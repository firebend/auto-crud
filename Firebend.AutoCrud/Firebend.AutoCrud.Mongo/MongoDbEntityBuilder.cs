using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoDbEntityBuilder : EntityCrudBuilder
    {
        private MongoDbEntityBuilder AddType(Type registrationType, Type serviceType, Type typeToCheck, params Type[] genericArguments)
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
            
            if (Registrations == null)
            {
                Registrations = new Dictionary<Type, Type>();
            }
            
            Registrations.Add(registrationType, serviceType);

            return this;
        }

        public MongoDbEntityBuilder WithCreate(Type registrationType, Type serviceType)
        {
            return AddType(registrationType,
                serviceType,
                typeof(IEntityCreateService<,>),
                EntityKeyType, EntityType);
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
        
        public MongoDbEntityBuilder WithRead(Type registrationType, Type serviceType)
        {
            return AddType(registrationType,
                serviceType,
                typeof(IEntityReadService<,>),
                EntityKeyType, EntityType);
        }

        public MongoDbEntityBuilder WithRead<TRegistration, TService>()
        {
            return WithRead(typeof(TRegistration), typeof(TService));
        }

        public MongoDbEntityBuilder WithRead()
        {
            var registrationType = typeof(IEntityReadService<,>).MakeGenericType(EntityKeyType, EntityType);
            var serviceType = typeof(MongoEntityReadService<,>).MakeGenericType(EntityKeyType, EntityType);

            return WithRead(registrationType, serviceType);
        }
        
        public MongoDbEntityBuilder WithSearch(Type registrationType, Type serviceType, Type searchType)
        {
            return AddType(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>),
                EntityKeyType, EntityType, serviceType);
        }

        public MongoDbEntityBuilder WithSearch<TRegistration, TService, TSearch>()
            where TSearch : EntitySearchRequest
        {
            return WithSearch(typeof(TRegistration), typeof(TService), typeof(TSearch));
        }

        public MongoDbEntityBuilder WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);
            
            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(EntityKeyType, EntityType, searchType);
            var serviceType = typeof(MongoEntitySearchService<,,>).MakeGenericType(EntityKeyType, EntityType, searchType);

            return WithSearch(registrationType, serviceType, searchType);
        }

        public MongoDbEntityBuilder WithSearch()
        {
            return WithSearch<EntitySearchRequest>();
        }
        
        public MongoDbEntityBuilder WithUpdate(Type registrationType, Type serviceType)
        {
            return AddType(registrationType,
                serviceType,
                typeof(IEntityUpdateService<,>),
                EntityKeyType, EntityType);
        }

        public MongoDbEntityBuilder WithUpdate<TRegistration, TService>()
        {
            return WithUpdate(typeof(TRegistration), typeof(TService));
        }

        public MongoDbEntityBuilder WithUpdate()
        {
            var registrationType = typeof(IEntityUpdateService<,>).MakeGenericType(EntityKeyType, EntityType);
            var serviceType = typeof(MongoEntityUpdateService<,>).MakeGenericType(EntityKeyType, EntityType);

            return WithUpdate(registrationType, serviceType);
        }
        
        public MongoDbEntityBuilder WithDelete(Type registrationType, Type serviceType)
        {
            return AddType(registrationType,
                serviceType,
                typeof(IEntityDeleteService<,>),
                EntityKeyType, EntityType);
        }

        public MongoDbEntityBuilder WithDelete<TRegistration, TService>()
        {
            return WithDelete(typeof(TRegistration), typeof(TService));
        }

        public MongoDbEntityBuilder WithDelete()
        {
            var registrationType = typeof(IEntityDeleteService<,>).MakeGenericType(EntityKeyType, EntityType);
            var serviceType = typeof(MongoEntityDeleteService<,>).MakeGenericType(EntityKeyType, EntityType);

            return WithDelete(registrationType, serviceType);
        }
    }
}