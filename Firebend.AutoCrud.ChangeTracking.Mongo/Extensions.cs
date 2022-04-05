using System;
using Firebend.AutoCrud.ChangeTracking.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo
{
    public static class Extensions
    {

        /// <summary>
        /// Adds change tracking for a given entity and persists it to a data store using Mongo.
        /// This function registers a <see cref="AbstractMongoChangeTrackingService{TEntityKey,TEntity}"/> to track changes and
        /// a <see cref="AbstractMongoChangeTrackingReadRepository{TEntityKey,TEntity}"/> to read changes.
        /// It also registers <see cref="AbstractChangeTrackingAddedDomainEventHandler{TKey,TEntity}"/>, <see cref="AbstractChangeTrackingUpdatedDomainEventHandler{TKey,TEntity}"/>,
        /// and <see cref="AbstractChangeTrackingDeleteDomainEventHandler{TKey,TEntity}"/> to hook into the domain event pipeline and persist the changes.
        /// <param name="configurator">
        /// The <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/> to configure Mongo persistence for.
        /// </param>
        /// <param name="changeTrackingOptions">
        /// The <see cref="ChangeTrackingOptions"/> to configure change tracking.
        /// </param>
        /// <typeparam name="TBuilder">
        /// The type of <see cref="MongoDbEntityBuilder{TKey,TEntity}"/> builder. Must inherit <see cref="MongoDbEntityBuilder{TKey,TEntity}"/>
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
        /// Throws an exception if <paramref name="configurator"/> does not implement <see cref="MongoDbEntityBuilder{TKey,TEntity}"/>
        /// </exception>
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithMongoChangeTracking<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> configurator,
            ChangeTrackingOptions changeTrackingOptions = null)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
        {
            if (configurator.Builder is not MongoDbEntityBuilder<TKey, TEntity> mongoDbEntityBuilder)
            {
                throw new Exception($"Configuration Error! This builder is not a {nameof(MongoDbEntityBuilder<Guid, FooEntity>)}");
            }

            configurator.Builder.WithRegistration<IMongoCreateClient<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                MongoCreateClient<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

            configurator.Builder.WithRegistration<IMongoReadClient<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                MongoReadClient<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

            configurator.Builder.WithRegistration<IMongoIndexClient<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                MongoIndexClient<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

            configurator.Builder.WithRegistration<IMongoIndexProvider<ChangeTrackingEntity<TKey, TEntity>>,
                AbstractMongoChangeTrackingIndexProvider<TKey, TEntity>>(false);

            configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity>,
                AbstractMongoChangeTrackingReadRepository<TKey, TEntity>>(false);

            configurator.Builder.WithRegistration<IChangeTrackingService<TKey, TEntity>,
                AbstractMongoChangeTrackingService<TKey, TEntity>>(false);

            configurator.Builder.WithRegistration<IDefaultEntityOrderByProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                DefaultEntityOrderByProviderModified<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

            configurator.Builder.WithRegistration<IEntityQueryOrderByHandler<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                DefaultEntityQueryOrderByHandler<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

            configurator.Builder.WithRegistration<
                IEntitySearchHandler<Guid, ChangeTrackingEntity<TKey, TEntity>, ChangeTrackingSearchRequest<TKey>>,
                MongoFullTextSearchHandler<Guid, ChangeTrackingEntity<TKey, TEntity>, ChangeTrackingSearchRequest<TKey>>>(false);

            if (configurator.Builder.IsTenantEntity)
            {
                configurator.Builder.WithRegistration<IMongoEntityConfigurationTenantTransformService<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                    MongoEntityConfigurationTenantTransformService<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

                configurator.Builder.WithRegistration<IMongoConfigurationAllShardsProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                    MongoConfigurationAllShardsProvider<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

                configurator.Builder.WithRegistration<IConfigureCollection<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                    MongoConfigureShardedCollection<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);

                configurator.Builder.WithRegistration<IConfigureCollection,
                    MongoConfigureShardedCollection<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false, true);

                configurator.Builder.WithRegistrationInstance<IMongoEntityDefaultConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>>(
                    new MongoEntityDefaultConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>(mongoDbEntityBuilder.CollectionName + "_ChangeTracking",
                        mongoDbEntityBuilder.Database,
                        mongoDbEntityBuilder.AggregateOption,
                        mongoDbEntityBuilder.ShardMode));

                configurator.Builder.WithRegistration<IMongoEntityConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                    MongoTenantEntityConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>>(false);
            }
            else
            {
                configurator.Builder.WithRegistration<IConfigureCollection<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                    MongoConfigureCollection<Guid, ChangeTrackingEntity<TKey, TEntity>>>();

                configurator.Builder.WithRegistration<IConfigureCollection, MongoConfigureCollection<Guid,
                    ChangeTrackingEntity<TKey, TEntity>>>(false, true);

                configurator.Builder.WithRegistrationInstance<IMongoEntityConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>>(
                    new MongoEntityConfiguration<Guid, ChangeTrackingEntity<TKey, TEntity>>(mongoDbEntityBuilder.CollectionName + "_ChangeTracking",
                        mongoDbEntityBuilder.Database,
                        mongoDbEntityBuilder.AggregateOption,
                        mongoDbEntityBuilder.ShardMode));
            }

            configurator.WithDomainEventEntityAddedSubscriber<AbstractChangeTrackingAddedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityUpdatedSubscriber<AbstractChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityDeletedSubscriber<AbstractChangeTrackingDeleteDomainEventHandler<TKey, TEntity>>();

            configurator.Builder.WithRegistrationInstance<IChangeTrackingOptionsProvider<TKey, TEntity>>(
                new DefaultChangeTrackingOptionsProvider<TKey, TEntity>(changeTrackingOptions ?? new ChangeTrackingOptions()));

            return configurator;
        }
    }
}
