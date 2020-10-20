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

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityCreateService<TKey, TEntity>));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate<TRegistration, TService>()
        {
            return WithCreate(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate()
        {
            var registrationType = typeof(IEntityCreateService<TKey, TEntity>);
            var serviceType = Builder.CreateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithCreate(registrationType, serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityDeleteService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete<TRegistration, TService>()
        {
            return WithDelete(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete()
        {
            var registrationType = typeof(IEntityDeleteService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            var deleteType = typeof(IActiveEntity).IsAssignableFrom(Builder.EntityType)
                ? Builder.SoftDeleteType
                : Builder.DeleteType;

            var serviceType = deleteType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithDelete(registrationType, serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy(Type type)
        {
            Builder.WithRegistration(
                typeof(IEntityDefaultOrderByProvider<TKey, TEntity>),
                type,
                typeof(IEntityDefaultOrderByProvider<TKey, TEntity>)
            );

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

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityReadService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead<TRegistration, TService>()
        {
            return WithRead(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead()
        {
            var registrationType = typeof(IEntityReadService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);
            var serviceType = Builder.ReadType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithRead(registrationType, serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch(Type registrationType, Type serviceType, Type searchType)
        {
            Builder.SearchRequestType = searchType;

            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TRegistration, TService, TSearch>()
            where TSearch : EntitySearchRequest
        {
            return WithSearch(typeof(TRegistration), typeof(TService), typeof(TSearch));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);

            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);
            var serviceType = Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            return WithSearch(registrationType, serviceType, searchType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch()
        {
            return WithSearch<EntitySearchRequest>();
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate(Type registrationType, Type serviceType)
        {
            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityUpdateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate<TRegistration, TService>()
        {
            return WithUpdate(typeof(TRegistration), typeof(TService));
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate()
        {
            var registrationType = typeof(IEntityUpdateService<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType);
            var serviceType = Builder.UpdateType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType);

            return WithUpdate(registrationType, serviceType);
        }
    }
}