using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class EntityBuilderExtensions
    {
        public static TBuilder ForEntity<TBuilder, TEntity, TEntityKey>(this TBuilder builder)
            where TBuilder : EntityBuilder
            where TEntity : IEntity<TEntityKey>
            where TEntityKey : struct
        {
            builder.EntityType = typeof(TEntity);
            builder.EntityKeyType = typeof(TEntityKey);

            return builder;
        }

        public static TBuilder WithEntityName<TBuilder>(this TBuilder builder, string entityName)
            where TBuilder : EntityBuilder
        {
            builder.EntityName = entityName;
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityBuilder
        {
            if (builder.Registrations == null)
            {
                builder.Registrations = new Dictionary<Type, Type> {{registrationType, serviceType}};
            }
            else
            {
                if (builder.Registrations.ContainsKey(registrationType))
                {
                    builder.Registrations[registrationType] = serviceType;
                }
                else
                {
                    builder.Registrations.Add(registrationType, serviceType);
                }
            }
            
            return builder;
        }

        public static TBuilder WithRegistration<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityBuilder
        {
            return builder.WithRegistration(typeof(TRegistration), typeof(TService));
        }
        
        public static TBuilder WithRegistration<TBuilder>(this TBuilder builder, Type  registrationType, Type serviceType, Type typeToCheck, params Type[] genericArguments)
            where TBuilder : EntityBuilder
        {
            var createType = typeToCheck.MakeGenericType(genericArguments);
            
            if (!registrationType.IsAssignableFrom(createType))
            {
                throw new ArgumentException($"Registration type is not assignable to {createType}");
            }
            
            if(serviceType.IsAssignableFrom(createType))
            {
                throw new ArgumentException($"Service type is not assignable to {createType}");
            }

            return builder.WithRegistration(serviceType, registrationType);
        }
    }
}