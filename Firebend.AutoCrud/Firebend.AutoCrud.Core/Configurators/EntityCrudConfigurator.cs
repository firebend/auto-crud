using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Configurators
{
    public class EntityCrudConfigurator<TBuilder> : BuilderConfigurator<TBuilder> where TBuilder : EntityCrudBuilder
    {
        public EntityCrudConfigurator(TBuilder builder) : base(builder)
        {
        }
        
        public EntityCrudConfigurator<TBuilder> WithCrud<TSearch>()
            where TSearch : EntitySearchRequest
        {
            WithCreate();
            WithRead();
            WithUpdate();
            WithDelete();
            WithSearch<TSearch>();
            
            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithCrud()
        {
            return WithCrud<EntitySearchRequest>();
        }
        
        public EntityCrudConfigurator<TBuilder> WithCreate(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityCreateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithCreate<TRegistration, TService>()
        {
            return WithCreate(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder> WithCreate()
        {
            var registrationType = typeof(IEntityCreateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);
            var serviceType = Builder.CreateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithCreate(registrationType, serviceType);
        }
        
        public EntityCrudConfigurator<TBuilder> WithDelete(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityDeleteService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithDelete<TRegistration, TService>()
        {
            return WithDelete(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder> WithDelete()
        {
            var registrationType = typeof(IEntityDeleteService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            var deleteType = typeof(IActiveEntity).IsAssignableFrom(Builder.EntityType)
                ? Builder.SoftDeleteType
                : Builder.DeleteType;

            var serviceType = deleteType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithDelete(registrationType, serviceType);
        }
        
        public EntityCrudConfigurator<TBuilder> WithOrderBy(Type type)
        {
            Builder.WithRegistration(
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType),
                type,
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType)
            );

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithOrderBy<T>()
        {
            return WithOrderBy(typeof(T));
        }
        
        private EntityCrudConfigurator<TBuilder> WithOrderBy<TEntity>((Expression<Func<TEntity, object>>, bool @ascending) orderBy)
        {
            var signature = $"{Builder.EntityType.Name}_{Builder.EntityName}_OrderBy";

            var iFaceType = typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityDefaultOrderByProvider<Guid, FooEntity>.OrderBy),
                Type = typeof((Expression<Func<TEntity, object>>, bool @ascending)),
                Value = orderBy
            };

            Builder.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { propertySet },
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });
            
            return this;
        }
        
        public EntityCrudConfigurator<TBuilder> WithRead(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityReadService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithRead<TRegistration, TService>()
        {
            return WithRead(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder> WithRead()
        {
            var registrationType = typeof(IEntityReadService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);
            var serviceType = Builder.ReadType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithRead(registrationType, serviceType);
        }
        
        public EntityCrudConfigurator<TBuilder> WithSearch(Type registrationType, Type serviceType, Type searchType)
        {
            Builder.SearchRequestType = searchType;

            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithSearch<TRegistration, TService, TSearch>()
            where TSearch : EntitySearchRequest
        {
            return WithSearch(typeof(TRegistration), typeof(TService), typeof(TSearch));
        }

        public EntityCrudConfigurator<TBuilder> WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);

            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);
            var serviceType = Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            return WithSearch(registrationType, serviceType, searchType);
        }

        public EntityCrudConfigurator<TBuilder> WithSearch()
        {
            return WithSearch<EntitySearchRequest>();
        }
        
        public EntityCrudConfigurator<TBuilder> WithUpdate(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityUpdateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder> WithUpdate<TRegistration, TService>()
        {
            return WithUpdate(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder> WithUpdate()
        {
            var registrationType = typeof(IEntityUpdateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);
            var serviceType = Builder.UpdateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithUpdate(registrationType, serviceType);
        }
    }
}