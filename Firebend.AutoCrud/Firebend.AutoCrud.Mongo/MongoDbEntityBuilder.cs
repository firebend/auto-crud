using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder : EntityCrudBuilder
    {
        private MongoDbEntityBuilder AddType(Type registrationType, Type serviceType, Type typeToCheck)
        {
            var createType = typeToCheck.MakeGenericType(EntityKeyType, EntityType);
            
            if (!registrationType.IsAssignableFrom(createType))
            {
                throw new ArgumentException($"Registration type is not assignable to {createType}");
            }
            
            if(serviceType.IsAssignableFrom(createType))
            {
                throw new ArgumentException($"Service type is not assignable to {createType}");
            }

            if (Registrations == null)
            {
                Registrations = new Dictionary<Type, Type>();
            }
            
            Registrations.Add(registrationType, serviceType);

            return this;
        }

        public MongoDbEntityBuilder WithCreate(Type registrationType, Type serviceType)
        {
            return AddType(registrationType, serviceType, typeof(IEntityCreateService<,>));
        }

        public MongoDbEntityBuilder WithCreate<TRegistration, TService>()
        {
            return WithCreate(typeof(TRegistration), typeof(TService));
        }

        public MongoDbEntityBuilder WithCreate()
        {
            var registrationType = typeof(IEntityCreateService<,>).MakeGenericType(EntityKeyType, EntityType);
            var serviceType = typeof(MongoEntityCreateService<,>).MakeGenericType(EntityKeyType, EntityType);

            return WithCreate(registrationType, serviceType);
        }
    }
}