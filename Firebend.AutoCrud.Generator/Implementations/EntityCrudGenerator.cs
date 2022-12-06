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

namespace Firebend.AutoCrud.Generator.Implementations
{
    public abstract class EntityCrudGenerator : BaseDisposable, IEntityCrudGenerator
    {
        private readonly object _lock = new();
        private bool _isGenerated;

        private readonly IDynamicClassGenerator _classGenerator;

        protected EntityCrudGenerator(IDynamicClassGenerator classGenerator, IServiceCollection serviceCollection)
        {
            _classGenerator = classGenerator;
            ServiceCollection = serviceCollection;
        }

        protected EntityCrudGenerator(IServiceCollection serviceCollection) : this(new DynamicClassGenerator(), serviceCollection)
        {
        }

        public List<BaseBuilder> Builders { get; private set; } = new();

        public IServiceCollection ServiceCollection { get; }

        public IServiceCollection Generate()
        {
            if (_isGenerated)
            {
                return ServiceCollection;
            }

            lock (_lock)
            {
                if (_isGenerated)
                {
                    return ServiceCollection;
                }

                OnGenerate();
                _isGenerated = true;
                return ServiceCollection;
            }
        }

        private void OnGenerate()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var builder in Builders)
            {
                var builderStopwatch = new Stopwatch();
                builderStopwatch.Start();
                Generate(ServiceCollection, builder);
                builderStopwatch.Stop();
                Console.WriteLine($"Generated entity crud for {builder.SignatureBase} in {builderStopwatch.ElapsedMilliseconds} (ms)");
                builder.Dispose();
            }

            stopwatch.Stop();

            Console.WriteLine($"All entities generated in {stopwatch.ElapsedMilliseconds} (ms)");
        }

        protected virtual void Generate(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            builder.Build();
            RegisterRegistrations(serviceCollection, builder);
            CallServiceCollectionHooks(serviceCollection, builder);
        }

        private static void CallServiceCollectionHooks(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            if (builder.ServiceCollectionHooks == null)
            {
                return;
            }

            foreach (var hook in builder.ServiceCollectionHooks)
            {
                hook(serviceCollection);
            }
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

                foreach (var reg in registrations)
                {
                    switch (reg)
                    {
                        case DynamicClassRegistration classRegistration:
                        {
                            var instance = _classGenerator.ImplementInterface(
                                classRegistration.Interface,
                                classRegistration.Signature,
                                classRegistration.Properties.ToArray());

                            serviceCollection.AddSingleton(classRegistration.Interface, instance);
                            break;
                        }
                        case InstanceRegistration instanceRegistration:
                            serviceCollection.AddSingleton(type, instanceRegistration.Instance);
                            break;
                        case ServiceRegistration serviceRegistration:
                            if (services.ContainsKey(type))
                            {
                                services[type] = services[type] ?? new List<ServiceRegistration>();
                                services[type].Add(serviceRegistration);
                            }
                            else
                            {
                                services.Add(type, new List<ServiceRegistration> { serviceRegistration });
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
                    throw new InvalidCastException($"Cannot use {value.Name} to implement {key.Name}");
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

            if (attributeArray.Any())
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
                        configureRegistrations[key] ??= new List<ServiceRegistration>();
                        configureRegistrations[key].Add(reg);
                    }
                    else
                    {
                        configureRegistrations.Add(key, new List<ServiceRegistration> { reg });
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
}
