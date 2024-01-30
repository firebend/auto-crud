using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Services;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.Generator.Implementations;

public abstract class EntityCrudGenerator : BaseDisposable, IEntityCrudGenerator
{
    private readonly object _lock = new();
    private bool _isGenerated;

    private readonly IDynamicClassGenerator _classGenerator;


    protected EntityCrudGenerator(IDynamicClassGenerator classGenerator, IServiceCollection services)
    {
        _classGenerator = classGenerator;
        Services = services;
    }

    protected EntityCrudGenerator(IServiceCollection services) : this(new DynamicClassGenerator(), services)
    {
    }

    public List<BaseBuilder> Builders { get; private set; } = [];

    public IServiceCollection Services { get; }

    public IServiceCollection Generate()
    {
        if (_isGenerated)
        {
            return Services;
        }

        lock (_lock)
        {
            if (_isGenerated)
            {
                return Services;
            }

            OnGenerate();
            _isGenerated = true;
            return Services;
        }
    }

    private void OnGenerate()
    {
        var start = Stopwatch.GetTimestamp();

        foreach (var builder in Builders)
        {
            var builderStart = Stopwatch.GetTimestamp();
            Generate(Services, builder);
            Console.WriteLine($"Generated entity crud for {builder.SignatureBase} in {Stopwatch.GetElapsedTime(builderStart).Milliseconds} (ms)");
            builder.Dispose();
        }

        Console.WriteLine($"All entities generated in {Stopwatch.GetElapsedTime(start).TotalMilliseconds} (ms)");
    }

    protected virtual void Generate(IServiceCollection serviceCollection, BaseBuilder builder)
    {
        builder.Build();
        RegisterRegistrations(serviceCollection, builder);
    }

    private void RegisterRegistrations(IServiceCollection serviceCollection, BaseBuilder builder)
    {
        var services = new Dictionary<Type, List<ServiceRegistration>>();

        foreach (var (type, registrations) in builder.Registrations)
        {
            if (registrations == null)
            {
                continue;
            }

            for (var index = 0; index < registrations.Count; index++)
            {
                var reg = registrations[index];

                switch (reg)
                {
                    case InstanceRegistration instanceRegistration:
                        serviceCollection.AddSingleton(type, instanceRegistration.Instance);
                        break;
                    case DynamicServiceRegistration serviceRegistration:
                        if (services.ContainsKey(type))
                        {
                            services[type] = services[type] ?? [];
                            services[type].Add(serviceRegistration);
                        }
                        else
                        {
                            services.Add(type, [serviceRegistration]);
                        }

                        break;
                    case ServiceRegistration serviceRegistration:

                        if (index <= 0)
                        {
                            serviceCollection.TryAdd(new ServiceDescriptor(type,
                                serviceRegistration.ServiceType,
                                serviceRegistration.Lifetime));
                        }
                        else
                        {
                            serviceCollection.Add(new ServiceDescriptor(type,
                                serviceRegistration.ServiceType,
                                serviceRegistration.Lifetime));
                        }

                        break;
                    case BuilderRegistration builderRegistration:
                        Generate(serviceCollection, builderRegistration.Builder);
                        break;
                }
            }
        }

        RegisterServiceRegistrations(serviceCollection, builder, services);
    }

    private void RegisterServiceRegistrations(IServiceCollection serviceCollection,
        BaseBuilder builder,
        IDictionary<Type, List<ServiceRegistration>> serviceRegistrations)
    {
        var signatureBase = builder.SignatureBase;
        var implementedTypes = new List<Type>();

        var extraInterfaces = GetCustomImplementations(serviceRegistrations);
        var ordered = OrderByDependencies(serviceRegistrations).Distinct().ToArray();

        foreach (var (key, value) in ordered)
        {
            var interfaceImplementations = extraInterfaces.FindAll(x =>
                x.IsAssignableFrom(value) && x.Name == $"I{value.Name}");

            if (!key.IsAssignableFrom(value))
            {
                throw new InvalidCastException($"Cannot use {value?.Name} to implement {key.Name}");
            }

            var signature = $"{signatureBase}_{value.Name}";

            if (key.IsInterface)
            {
                interfaceImplementations.Add(key);
                interfaceImplementations.Add(_classGenerator.GenerateInterface(key, $"I{signature}"));
            }

            interfaceImplementations = interfaceImplementations.Distinct().ToList();

            try
            {
                var implementedType = _classGenerator.GenerateDynamicClass(
                    value,
                    signature,
                    implementedTypes,
                    interfaceImplementations.ToArray(),
                    GetAttributes(value, builder.Attributes));


                interfaceImplementations.ForEach(iFace => serviceCollection.AddScoped(iFace, implementedType));

                if (interfaceImplementations.Count == 0)
                {
                    serviceCollection.AddScoped(implementedType);
                }

                implementedTypes = implementedTypes.Union(interfaceImplementations).Distinct().ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private static CustomAttributeBuilder[] GetAttributes(Type typeToImplement, IDictionary<Type, List<CrudBuilderAttributeModel>> builderAttributes)
    {
        if (builderAttributes == null)
        {
            return null;
        }

        var attributes = new List<CustomAttributeBuilder>();

        foreach (var (type, attribute) in builderAttributes)
        {
            if (type.IsAssignableFrom(typeToImplement))
            {
                attributes.AddRange(attribute.Select(x => x.AttributeBuilder));
            }
        }

        var attributeArray = attributes.Distinct().ToArray();

        if (attributeArray.Length != 0)
        {
            return attributeArray;
        }

        return null;
    }

    private static IEnumerable<KeyValuePair<Type, Type>> OrderByDependencies(IDictionary<Type, List<ServiceRegistration>> source)
    {
        var orderedTypes = new List<KeyValuePair<Type, Type>>();

        if (source != null)
        {
            var maxVisits = source.Count;

            var typesToAdd = source
                .SelectMany(x => x.Value, (pair, registration) => new KeyValuePair<Type, Type>(pair.Key, registration.ServiceType))
                .ToList();

            while (typesToAdd.Count > 0)
            {
                foreach (var type in typesToAdd.ToArray())
                {
                    if (CanAddType(type, typesToAdd))
                    {
                        orderedTypes.Add(type);
                        typesToAdd.Remove(type);
                    }
                }

                maxVisits--;

                if (maxVisits < 0)
                {
                    throw new ApplicationException("Cannot resolve dependencies for auto crud (do you have a circular reference?)");
                }
            }
        }

        return orderedTypes;
    }

    private static bool CanAddType(KeyValuePair<Type, Type> type, List<KeyValuePair<Type, Type>> typesToAdd) => type.Value.GetConstructors(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance)
        .All(
            info => info
                .GetParameters()
                .All(parameterInfo =>
                    typesToAdd.All(t => t.Key != parameterInfo.ParameterType)
                    && typesToAdd.All(types => !parameterInfo.ParameterType.IsAssignableFrom(types.Key))
                )
        );

    private static List<Type> GetCustomImplementations(IDictionary<Type, List<ServiceRegistration>> configureRegistrations)
    {
        var extraInterfaces = new List<Type>();

        if (configureRegistrations == null)
        {
            return extraInterfaces;
        }

        foreach (var (key, regs) in configureRegistrations.ToArray())
        {
            foreach (var reg in regs.ToArray())
            {
                if (!key.IsAssignableFrom(reg.ServiceType))
                {
                    var args = reg.ServiceType.GenericTypeArguments.Aggregate(new StringBuilder(), (a, b) => a.Append(b.Name).Append(","));
                    var args2 = key.GenericTypeArguments.Aggregate(new StringBuilder(), (a, b) => a.Append(b.Name).Append(","));

                    var argsStr = args.Length > 0 ? args.ToString(0, args.Length - 1) : string.Empty;
                    var args2Str = args2.Length > 0 ? args2.ToString(0, args2.Length - 1) : string.Empty;

                    throw new InvalidCastException($"Cannot use custom configuration {reg.ServiceType.Name} to implement {key.Name}. {argsStr} {args2Str}");
                }

                var implementedInterfaces = reg.ServiceType.GetInterfaces();
                var matchingInterface = implementedInterfaces.FirstOrDefault(x => x.Name == $"I{reg.ServiceType.Name}");

                if (matchingInterface != null)
                {
                    extraInterfaces.Add(matchingInterface);
                }

                if (configureRegistrations.ContainsKey(key))
                {
                    configureRegistrations[key] ??= [];
                    configureRegistrations[key].Add(reg);
                }
                else
                {
                    configureRegistrations.Add(key, [reg]);
                }
            }
        }

        return extraInterfaces;
    }

    public EntityCrudGenerator AddBuilder<TBuilder>(TBuilder builder, Func<TBuilder, TBuilder> configure)
        where TBuilder : BaseBuilder, new()

    {
        configure(new TBuilder());

        Builders.Add(builder);

        return this;
    }

    protected override void DisposeManagedObjects()
    {
        _classGenerator?.Dispose();

        if (Builders is not null)
        {
            foreach (var builder in Builders)
            {
                builder.Dispose();
            }

            Builders.Clear();
        }

        Builders = null;
    }
}
