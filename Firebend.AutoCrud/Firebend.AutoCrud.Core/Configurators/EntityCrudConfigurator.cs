using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Configurators
{
    public class EntityCrudConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
    {
        public EntityCrudConfigurator(TBuilder builder) : base(builder)
        {
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCrud<TSearch>()
            where TSearch : EntitySearchRequest
        {
            WithCreate();
            WithRead();
            WithUpdate();
            WithDelete();
            WithSearch<TSearch>();

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCrud()
        {
            return WithCrud<EntitySearchRequest>();
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate(Type serviceType)
        {
            Builder.WithRegistration<IEntityCreateService<TKey, TEntity>>(serviceType);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate<TService>()
        {
            return WithCreate(typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate()
        {
            var serviceType = Builder.CreateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithCreate(serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete(Type serviceType)
        {
            Builder.WithRegistration<IEntityDeleteService<TKey, TEntity>>(serviceType);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete<TService>()
        {
            return WithDelete(typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete()
        {
            var deleteType = typeof(IActiveEntity).IsAssignableFrom(Builder.EntityType)
                ? Builder.SoftDeleteType
                : Builder.DeleteType;

            var serviceType = deleteType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithDelete(serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy(Type type)
        {
            Builder.WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>>(type);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy<T>()
        {
            return WithOrderBy(typeof(T));
        }

        // ReSharper disable once UnusedMember.Local
        private EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy((Expression<Func<TEntity, object>>, bool ascending) orderBy)
        {
            var signature = $"{Builder.SignatureBase}_OrderBy";

            var iFaceType = typeof(IEntityDefaultOrderByProvider<TKey, TEntity>);

            var propertySet = new PropertySet<(Expression<Func<TEntity, object>>, bool ascending)>
            {
                Name = nameof(IEntityDefaultOrderByProvider<Guid, FooEntity>.OrderBy),
                Value = orderBy,
                Override = true
            };

            Builder.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new[] {propertySet},
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead(Type serviceType)
        {
            Builder.WithRegistration<IEntityReadService<TKey, TEntity>>(serviceType);
            
            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead<TService>()
        {
            return WithRead(typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead()
        {
            var serviceType = Builder.ReadType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithRead(serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch(Type serviceType, Type searchType)
        {
            Builder.SearchRequestType = searchType;

            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);
            
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TService, TSearch>()
            where TSearch : EntitySearchRequest
        {
            return WithSearch(typeof(TService), typeof(TSearch));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);

            var serviceType = Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            return WithSearch(serviceType, searchType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch()
        {
            return WithSearch<EntitySearchRequest>();
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate(Type serviceType)
        {
            Builder.WithRegistration<IEntityUpdateService<TKey, TEntity>>(serviceType);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate<TService>()
        {
            return WithUpdate(typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate()
        {
            var serviceType = Builder.UpdateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithUpdate(serviceType);
        }
    }
}