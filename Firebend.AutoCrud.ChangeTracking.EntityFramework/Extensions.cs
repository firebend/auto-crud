using System;
using Firebend.AutoCrud.ChangeTracking.Abstractions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Searching;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework
{
    public static class Extensions
    {
        /// <summary>
        /// Adds change tracking for a given entity and persists it to a data store using Entity Framework.
        /// This function registers a <see cref="AbstractEntityFrameworkChangeTrackingService{TEntityKey,TEntity}"/> to track changes and
        /// a <see cref="AbstractEntityFrameworkChangeTrackingReadService{TEntityKey,TEntity}"/> to read changes.
        /// It also registers <see cref="AbstractChangeTrackingAddedDomainEventHandler{TKey,TEntity}"/>, <see cref="AbstractChangeTrackingUpdatedDomainEventHandler{TKey,TEntity}"/>,
        /// and <see cref="AbstractChangeTrackingDeleteDomainEventHandler{TKey,TEntity}"/> to hook into the domain event pipeline and persist the changes.
        /// A <see cref="AbstractChangeTrackingDbContextProvider{TEntityKey,TEntity,TContext}"/> is registered so that a special change tracking Entity Framework context
        /// can be used to persist the changes.
        /// </summary>
        /// <param name="configurator">
        /// The <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/> to configure Entity Framework persistence for.
        /// </param>
        /// <param name="changeTrackingOptions">
        /// The <see cref="ChangeTrackingOptions"/> to configure change tracking.
        /// </param>
        /// <typeparam name="TBuilder">
        /// The type of <see cref="EntityCrudBuilder{TKey,TEntity}"/> builder. Must inherit <see cref="EntityFrameworkEntityBuilder{TKey,TEntity}"/>
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of key the entity uses.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// The type of entity.
        /// </typeparam>
        /// <returns>
        /// A <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/>
        /// </returns>
        /// <exception cref="Exception">
        /// Throws an exception if <paramref name="configurator"/> does not implement <see cref="EntityFrameworkEntityBuilder{TKey,TEntity}"/>
        /// </exception>
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithEfChangeTracking<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> configurator,
            ChangeTrackingOptions changeTrackingOptions = null
            )
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
        {
            if (configurator.Builder is not EntityFrameworkEntityBuilder<TKey, TEntity> efBuilder)
            {
                throw new Exception($"Configuration Error! This builder is not a {nameof(EntityFrameworkEntityBuilder<Guid, FooEntity>)}");
            }

            if (efBuilder.DbContextType is null)
            {
                throw new Exception("Please configure the builder's db context first.");
            }

            var providerType = (efBuilder.IsTenantEntity ?
                typeof(AbstractElasticChangeTrackingDbContextProvider<,,>):
                typeof(AbstractChangeTrackingDbContextProvider<,,>))
                .MakeGenericType(efBuilder.EntityKeyType, efBuilder.EntityType, efBuilder.DbContextType);

            configurator.Builder.WithRegistration<IChangeTrackingDbContextProvider<TKey, TEntity>>(providerType);

            configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity>,
                AbstractEntityFrameworkChangeTrackingReadService<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingService<TKey, TEntity>,
                AbstractEntityFrameworkChangeTrackingService<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                EntityFrameworkQueryClient<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

            configurator.Builder.WithRegistration<IDefaultEntityOrderByProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                DefaultEntityOrderByProviderModified<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

            configurator.Builder.WithRegistration<IEntityQueryOrderByHandler<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                DefaultEntityQueryOrderByHandler<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

            configurator.Builder.WithRegistration<IEntitySearchHandler<Guid, ChangeTrackingEntity<TKey, TEntity>, ChangeTrackingSearchRequest<TKey>>,
                EntityFrameworkChangeTrackingSearchHandler<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

            configurator.WithDomainEventEntityAddedSubscriber<AbstractChangeTrackingAddedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityUpdatedSubscriber<AbstractChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityDeletedSubscriber<AbstractChangeTrackingDeleteDomainEventHandler<TKey, TEntity>>();

            configurator.Builder.WithRegistrationInstance<IChangeTrackingOptionsProvider<TKey, TEntity>>(
                    new DefaultChangeTrackingOptionsProvider<TKey, TEntity>(changeTrackingOptions ?? new ChangeTrackingOptions()));

            return configurator;
        }
    }
}
