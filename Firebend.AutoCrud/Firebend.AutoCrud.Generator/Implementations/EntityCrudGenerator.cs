using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Services;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Generator.Implementations
{
    public abstract class EntityCrudGenerator : IEntityCrudGenerator
    {
        private readonly IDynamicClassGenerator _classGenerator;

        protected EntityCrudGenerator(IDynamicClassGenerator classGenerator, IServiceCollection serviceCollection)
        {
            _classGenerator = classGenerator;
            ServiceCollection = serviceCollection;
        }

        protected EntityCrudGenerator(IServiceCollection serviceCollection) : this(new DynamicClassGenerator(), serviceCollection)
        {
        }

        public List<BaseBuilder> Builders { get; } = new List<BaseBuilder>();

        public IServiceCollection ServiceCollection { get; }

        public IServiceCollection Generate()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Parallel.ForEach(Builders, builder =>
            {
                var builderStopwatch = new Stopwatch();
                builderStopwatch.Start();
                Generate(ServiceCollection, builder);
                builderStopwatch.Stop();
                Console.WriteLine($"Generated entity crud for {builder.SignatureBase} in {builderStopwatch.ElapsedMilliseconds} (ms)");
            });
            
            stopwatch.Stop();
            Console.WriteLine($"All entities generated in {stopwatch.ElapsedMilliseconds} (ms)");

            return ServiceCollection;
        }

        protected virtual void Generate(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            RegisterRegistrations(serviceCollection, builder);
            RegisterInstances(serviceCollection, builder);
            RegisterDynamicClasses(serviceCollection, builder);
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

        private void RegisterDynamicClasses(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            foreach (var reg in builder
                .Registrations
                .Where(x => x.Value is DynamicClassRegistration))
            {
                var classRegistration = reg.Value as DynamicClassRegistration;

                if (classRegistration == null)
                {
                    continue;
                }
                
                var instance = _classGenerator
                    .ImplementInterface(classRegistration.Interface, classRegistration.Signature, classRegistration.Properties.ToArray());

                serviceCollection.AddSingleton(classRegistration.Interface, instance);
            }
            
        }

        private void RegisterRegistrations(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            var signatureBase = builder.SignatureBase;
            var implementedTypes = new List<Type>();

            builder.Build();

            var serviceRegistrations = builder.Registrations.Where(x => x.Value is ServiceRegistration)
                .ToDictionary(x => x.Key, x => (ServiceRegistration) x.Value);

            var extraInterfaces = GetCustomImplementations(serviceRegistrations);

            foreach (var (key, value) in OrderByDependencies(serviceRegistrations))
            {
                var typeToImplement = value;
                var interfaceImplementations = extraInterfaces.FindAll(x =>
                    x.IsAssignableFrom(typeToImplement) && x.Name == $"I{typeToImplement.Name}");

                if (!key.IsAssignableFrom(typeToImplement)) throw new InvalidCastException($"Cannot use {typeToImplement.Name} to implement {key.Name}");

                var signature = $"{signatureBase}_{typeToImplement.Name}";

                if (key.IsInterface)
                {
                    interfaceImplementations.Add(key);
                    interfaceImplementations.Add(_classGenerator.GenerateInterface(key, $"I{signature}"));
                }

                interfaceImplementations = interfaceImplementations.Distinct().ToList();

                var implementedType = _classGenerator.GenerateDynamicClass(
                    typeToImplement,
                    signature,
                    implementedTypes,
                    interfaceImplementations.ToArray(),
                    GetAttributes(typeToImplement, builder.Attributes));

                interfaceImplementations.ForEach(iFace => { serviceCollection.AddScoped(iFace, implementedType); });

                if (interfaceImplementations.Count == 0) serviceCollection.AddScoped(implementedType);

                implementedTypes = implementedTypes.Union(interfaceImplementations).Distinct().ToList();
            }
        }

        private static void RegisterInstances(IServiceCollection serviceCollection, BaseBuilder builder)
        {
            foreach (var (key, value) in builder.Registrations
                .Where(x => x.Value is InstanceRegistration))
            {
                var instance = (value as InstanceRegistration)?.Instance;

                if (instance != null)
                {
                    serviceCollection.AddSingleton(key, instance);
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

        private static IEnumerable<KeyValuePair<Type, Type>> OrderByDependencies(IDictionary<Type, ServiceRegistration> source)
        {
            var orderedTypes = new List<KeyValuePair<Type, Type>>();

            if (source != null)
            {
                var maxVisits = source.Count;

                var typesToAdd = source
                    .ToDictionary(x => x.Key, x => x.Value.ServiceType);

                while (typesToAdd.Count > 0)
                {
                    foreach (var type in typesToAdd.ToArray())
                    {
                        if (CanAddType(type, typesToAdd))
                        {
                            orderedTypes.Add(type);
                            typesToAdd.Remove(type.Key);
                        }
                    }

                    maxVisits--;

                    if (maxVisits < 0) throw new ApplicationException("Cannot resolve dependencies for DefaultCrud (do you have a circular reference?)");
                }
            }

            return orderedTypes;
        }

        private static bool CanAddType(KeyValuePair<Type, Type> type, IDictionary<Type, Type> typesToAdd)
        {
            return type.Value.GetConstructors(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance)
                .All(
                    info => info.GetParameters().All(parameterInfo =>
                        !typesToAdd.ContainsKey(parameterInfo.ParameterType) &&
                        typesToAdd
                            .All(types => !parameterInfo.ParameterType.IsAssignableFrom(types.Key))
                    )
                );
        }

        private static List<Type> GetCustomImplementations(IDictionary<Type, ServiceRegistration> configureRegistrations)
        {
            var extraInterfaces = new List<Type>();

            if (configureRegistrations != null)
                
                foreach (var (key, reg) in configureRegistrations.ToArray())
                {
                    var value = reg.ServiceType;
                    
                    if (!key.IsAssignableFrom(value))
                    {
                        throw new InvalidCastException($"Cannot use custom configuration {value.Name} to implement {key.Name}");
                    }

                    var implementedInterfaces = value.GetInterfaces();
                    var matchingInterface =
                        implementedInterfaces.FirstOrDefault(x => x.Name == $"I{value.Name}");

                    if (matchingInterface != null) extraInterfaces.Add(matchingInterface);

                    if (configureRegistrations.ContainsKey(key))
                    {
                        configureRegistrations[key] = reg;
                    }
                    else
                    {
                        configureRegistrations.Add(key, reg);
                    }
                }

            return extraInterfaces;
        }

        public EntityCrudGenerator AddBuilder<TBuilder>(TBuilder builder, Func<TBuilder, TBuilder> configure) 
            where TBuilder : BaseBuilder,  new()

        {
            configure(new TBuilder());

            Builders.Add(builder);

            return this;
        }
    }
}