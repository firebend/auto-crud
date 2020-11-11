using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;

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
            WithCreate();
            WithRead();
            WithUpdate();
            WithDelete();
            WithSearch(Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.SearchRequestType), Builder.SearchRequestType);

            return this;
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
            var serviceType = Builder.CreateType;

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
            var serviceType = Builder.DeleteType;

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

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy(Expression<Func<TEntity, object>> expression, bool isAscending = true)
        {
            var instance = new DefaultEntityDefaultOrderByProvider<TKey, TEntity>
            {
                OrderBy = (
                    expression,
                    isAscending
                )
            };

            Builder.WithRegistrationInstance<IEntityDefaultOrderByProvider<TKey, TEntity>>(instance);

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
            var serviceType = Builder.ReadType;

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
            var serviceType = Builder.UpdateType;

            return WithUpdate(serviceType);
        }
    }
}
