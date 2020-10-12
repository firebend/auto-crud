using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
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
            builder.EntityName = builder.EntityType.Name;

            return builder;
        }

        public static TBuilder WithEnityName<TBuilder>(this TBuilder builder, string entityName)
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
        
        public static TBuilder WithRegistration<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType, Type typeToCheck)
            where TBuilder : EntityBuilder
        {
            if (!typeToCheck.IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Registration type is not assignable to {typeToCheck}");
            }
            
            if(!typeToCheck.IsAssignableFrom(registrationType))
            {
                throw new ArgumentException($"Service type is not assignable to {typeToCheck}");
            }

            return builder.WithRegistration(registrationType, serviceType);
        }

        public static TBuilder WithRegistrationInstance<TBuilder>(this TBuilder builder, Type registrationType, object instance)
            where TBuilder : EntityBuilder
        {
            if (builder.InstanceRegistrations == null)
            {
                builder.InstanceRegistrations = new Dictionary<Type, object> {{registrationType, instance}};
            }
            else
            {
                if (builder.InstanceRegistrations.ContainsKey(registrationType))
                {
                    builder.InstanceRegistrations[registrationType] = instance;
                }
                else
                {
                    builder.InstanceRegistrations.Add(registrationType, instance);
                }
            }
            
            return builder;
        }
    }
}