using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Generator.Implementations
{
    public abstract class EntityCrudGenerator<TBuilder> : IEntityCrudGenerator
        where TBuilder : EntityBuilder, new()
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

        public List<EntityBuilder> Builders { get; } = new List<EntityBuilder>();

        public IServiceCollection ServiceCollection { get; }

        public IServiceCollection Generate()
        {
            foreach (var builder in Builders) Generate(ServiceCollection, builder);

            return ServiceCollection;
        }

        protected virtual void Generate(IServiceCollection serviceCollection, EntityBuilder builder)
        {
            var signatureBase = $"{builder.EntityType.Name}_{builder.EntityName}";
            var implementedTypes = new List<Type>();

            builder.Build();

            var extraInterfaces = GetCustomImplementations(builder.Registrations);

            foreach (var (key, value) in OrderByDependencies(builder.Registrations))
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

            if (builder.InstanceRegistrations != null)
                foreach (var (key, value) in builder.InstanceRegistrations)
                    serviceCollection.AddSingleton(key, value);
        }

        private static CustomAttributeBuilder[] GetAttributes(Type typeToImplement, IDictionary<Type, List<CustomAttributeBuilder>> builderAttributes)
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
                    attributes.AddRange(attribute);
                }
            }

            return attributes.ToArray();
        }

        private static IEnumerable<KeyValuePair<Type, Type>> OrderByDependencies(IDictionary<Type, Type> source)
        {
            var orderedTypes = new List<KeyValuePair<Type, Type>>();

            if (source != null)
            {
                var maxVisits = source.Count;

                var typesToAdd = source.ToDictionary(x => x.Key, x => x.Value);

                while (typesToAdd.Count > 0)
                {
                    foreach (var type in typesToAdd.ToArray())
                        if (CanAddType(type, typesToAdd))
                        {
                            orderedTypes.Add(type);
                            typesToAdd.Remove(type.Key);
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

        private static List<Type> GetCustomImplementations(IDictionary<Type, Type> configureRegistrations)
        {
            var extraInterfaces = new List<Type>();

            if (configureRegistrations != null)
                foreach (var (key, value) in configureRegistrations.ToArray())
                {
                    if (!key.IsAssignableFrom(value))
                        throw new InvalidCastException(
                            $"Cannot use custom configuration {value.Name} to implement {key.Name}");

                    var implementedInterfaces = value.GetInterfaces();
                    var matchingInterface =
                        implementedInterfaces.FirstOrDefault(x => x.Name == $"I{value.Name}");

                    if (matchingInterface != null) extraInterfaces.Add(matchingInterface);

                    if (configureRegistrations.ContainsKey(key))
                        configureRegistrations[key] = value;
                    else
                        configureRegistrations.Add(key, value);
                }

            return extraInterfaces;
        }

        public EntityCrudGenerator<TBuilder> AddBuilder(TBuilder builder, Func<TBuilder, TBuilder> configure = null)
        {
            if (configure != null) builder = configure(builder);

            Builders.Add(builder);

            return this;
        }

        public EntityCrudGenerator<TBuilder> AddBuilder<T>(Func<TBuilder, TBuilder> configure)

        {
            var builder = configure(new TBuilder());

            return AddBuilder(builder, configure);
        }

        public EntityCrudGenerator<TBuilder> AddBuilder<TEntity, TEntityKey>(Func<TBuilder, TBuilder> configure)
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            var builder = configure(new TBuilder().ForEntity<TBuilder, TEntity, TEntityKey>());

            return AddBuilder(builder, configure);
        }
    }
}