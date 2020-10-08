using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class EntityCrudBuilderExtensions
    {
        public static TBuilder ForEntity<TBuilder, TEntity, TEntityKey>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.EntityType = typeof(TEntity);
            builder.EntityKeyType = typeof(TEntityKey);

            return builder;
        }

        public static TBuilder WithEntityName<TBuilder>(this TBuilder builder, string entityName)
            where TBuilder : EntityCrudBuilder
        {
            builder.EntityName = entityName;
            return builder;
        }

        public static TBuilder WithRoute<TBuilder>(this TBuilder builder, string route)
            where TBuilder : EntityCrudBuilder
        {
            builder.RoutePrefix = route;
            return builder;
        }

        public static TBuilder WithGetAllEndpoint<TBuilder>(this TBuilder builder, bool getAll)
            where TBuilder : EntityCrudBuilder
        {
            builder.IncludeGetAllEndpoint = getAll;
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityCrudBuilder
        {
            builder.Registrations ??= new Dictionary<Type, Type>();
            
            builder.Registrations.Add(registrationType, serviceType);
            
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(typeof(TRegistration), typeof(TService));
        }
    }
}