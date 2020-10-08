using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudBuilder
    {
        /// <summary>
        /// Gets a value indicating the <see cref="Type"/> of Entity.
        /// </summary>
        public Type EntityType { get; internal set; }
        
        /// <summary>
        /// Gets value indicating the <see cref="Type"/> key for the entity.
        /// </summary>
        public Type EntityKeyType { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating a friendly name for the entity used in routes.
        /// </summary>
        public string EntityName { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating the entity route prefix. i.e api/v1/
        /// </summary>
        public string RoutePrefix { get; internal set; }
        
        /// <summary>
        /// Gets a value indicating whether or not a GET endpoint will be exposed that allows for all entities to be retrieved at once.
        /// </summary>
        public bool IncludeGetAllEndpoint { get; internal set; }
        
        public IDictionary<Type, Type> Registrations { get; internal set; }
        
        public IDictionary<Type, string> ControllerPolicies { get; internal set; }
    }

    public static class EntityCrudBuilderExtensions
    {
        public static TBuilder ForEntity<TBuilder, TEntity, TEntityKey>(this TBuilder builder)
            where TBuilder : IEntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.EntityType = typeof(TEntity);
            builder.EntityKeyType = typeof(TEntityKey);

            return builder;
        }

        public static TBuilder WithEntityName<TBuilder>(this TBuilder builder, string entityName)
            where TBuilder : IEntityCrudBuilder
        {
            builder.EntityName = entityName;
            return builder;
        }

        public static TBuilder WithRoute<TBuilder>(this TBuilder builder, string route)
            where TBuilder : IEntityCrudBuilder
        {
            builder.RoutePrefix = route;
            return builder;
        }

        public static TBuilder WithGetAllEndpoint<TBuilder>(this TBuilder builder, bool getAll)
            where TBuilder : IEntityCrudBuilder
        {
            builder.IncludeGetAllEndpoint = getAll;
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : IEntityCrudBuilder
        {
            builder.Registrations ??= new Dictionary<Type, Type>();
            
            builder.Registrations.Add(registrationType, serviceType);
            
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : IEntityCrudBuilder
        {
            return builder.WithRegistration(typeof(TRegistration), typeof(TService));
        }
    }
}