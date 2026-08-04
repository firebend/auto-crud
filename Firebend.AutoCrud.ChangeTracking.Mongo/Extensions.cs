using System;
using Firebend.AutoCrud.ChangeTracking.Implementations;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Client.Configuration;
using Firebend.AutoCrud.Mongo.Client.Crud;
using Firebend.AutoCrud.Mongo.Client.Indexing;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo;

public static class Extensions
{
    /// <summary>
    /// Adds change tracking for a given entity and persists it to a data store using Mongo,
    /// using the default <see cref="ChangeTrackingEntity{TKey,TEntity}"/> row type.
    /// This function registers a <see cref="MongoChangeTrackingService{TEntityKey,TEntity,TChangeTrackingEntity}"/> to track changes and
    /// a <see cref="MongoChangeTrackingReadRepository{TEntityKey,TEntity}"/> to read changes.
    /// It also registers <see cref="ChangeTrackingAddedDomainEventHandler{TKey,TEntity}"/>, <see cref="ChangeTrackingUpdatedDomainEventHandler{TKey,TEntity}"/>,
    /// and <see cref="ChangeTrackingDeleteDomainEventHandler{TKey,TEntity}"/> to hook into the domain event pipeline and persist the changes.
    /// <param name="configurator">
    /// The <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/> to configure Mongo persistence for.
    /// </param>
    /// <param name="configure">
    /// A function to configure mongo change tracking.
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
        Action<MongoChangeTrackingConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, ChangeTrackingEntity<TKey, TEntity>>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        => configurator.WithMongoChangeTracking<TBuilder, TKey, TEntity, ChangeTrackingEntity<TKey, TEntity>>(configure);

    /// <summary>
    /// Adds change tracking for a given entity and persists it to a data store using Mongo,
    /// using a custom <typeparamref name="TChangeTrackingEntity"/> row type. Implement
    /// <see cref="IAuditContextProperties"/> on <typeparamref name="TChangeTrackingEntity"/> to populate
    /// its own additional fields from the domain event context.
    /// This function registers a <see cref="MongoChangeTrackingService{TEntityKey,TEntity,TChangeTrackingEntity}"/> to track changes and,
    /// when <typeparamref name="TChangeTrackingEntity"/> is the default <see cref="ChangeTrackingEntity{TKey,TEntity}"/> row type,
    /// a <see cref="MongoChangeTrackingReadRepository{TEntityKey,TEntity}"/> to read changes.
    /// It always also registers a <see cref="MongoChangeTrackingReadRepository{TEntityKey,TEntity,TChangeTrackingEntity}"/>
    /// keyed on <typeparamref name="TChangeTrackingEntity"/>, so a consumer using a custom row type can still read
    /// changes back (including its extra fields) via <c>WithChangeTrackingControllers</c>.
    /// It also registers <see cref="ChangeTrackingAddedDomainEventHandler{TKey,TEntity}"/>, <see cref="ChangeTrackingUpdatedDomainEventHandler{TKey,TEntity}"/>,
    /// and <see cref="ChangeTrackingDeleteDomainEventHandler{TKey,TEntity}"/> to hook into the domain event pipeline and persist the changes.
    /// <param name="configurator">
    /// The <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/> to configure Mongo persistence for.
    /// </param>
    /// <param name="configure">
    /// A function to configure mongo change tracking.
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
    /// <typeparam name="TChangeTrackingEntity">
    /// The type of row persisted for each change. Must inherit <see cref="ChangeTrackingEntity{TKey,TEntity}"/>. Implement
    /// <see cref="IAuditContextProperties"/> on this type to populate its own additional fields from the domain event context.
    /// </typeparam>
    /// <returns>
    /// A <see cref="DomainEventsConfigurator{TBuilder,TKey,TEntity}"/>
    /// </returns>
    /// <exception cref="Exception">
    /// Throws an exception if <paramref name="configurator"/> does not implement <see cref="MongoDbEntityBuilder{TKey,TEntity}"/>
    /// </exception>
    public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithMongoChangeTracking<TBuilder, TKey, TEntity, TChangeTrackingEntity>(
        this DomainEventsConfigurator<TBuilder, TKey, TEntity> configurator,
        Action<MongoChangeTrackingConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TChangeTrackingEntity>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>, new()
    {
        if (configurator.Builder is not MongoDbEntityBuilder<TKey, TEntity> mongoDbEntityBuilder)
        {
            throw new Exception($"Configuration Error! This builder is not a {nameof(MongoDbEntityBuilder<Guid, FooEntity>)}");
        }

        configurator.Builder.WithRegistration<IMongoCreateClient<Guid, TChangeTrackingEntity>,
            MongoCreateClient<Guid, TChangeTrackingEntity>>();

        configurator.Builder.WithRegistration<IMongoIndexClient<Guid, TChangeTrackingEntity>,
            MongoIndexClient<Guid, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<IMongoIndexProvider<Guid, TChangeTrackingEntity>,
            MongoChangeTrackingIndexProvider<TKey, TEntity, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<IChangeTrackingService<TKey, TEntity>,
            MongoChangeTrackingService<TKey, TEntity, TChangeTrackingEntity>>(false);

        // The old 2-arg IChangeTrackingReadService<TKey,TEntity> interface predates custom row
        // types and can only be wired up when the row type in use actually is the base
        // ChangeTrackingEntity<TKey,TEntity> type - mirroring the EF implementation for
        // consistency, even though Mongo itself has no equivalent CLR-base-class query
        // restriction. Its own dependencies (Mongo read client, order-by, search handler) are NOT
        // registered here - the unconditional block below always supplies them, keyed on
        // TChangeTrackingEntity, and when TChangeTrackingEntity is this same default type, that's
        // exactly what this registration needs too, so registering them twice would be redundant.
        if (typeof(TChangeTrackingEntity) == typeof(ChangeTrackingEntity<TKey, TEntity>))
        {
            configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity>,
                MongoChangeTrackingReadRepository<TKey, TEntity>>(false);
        }

        // Registers the tier-2/3 read path - the 3-arg IChangeTrackingReadService and everything it
        // depends on (Mongo read client, order-by, search handler) - keyed on TChangeTrackingEntity
        // itself, so it works for both the default and any custom row type. When TChangeTrackingEntity
        // is the default ChangeTrackingEntity<TKey,TEntity>, these same registrations also satisfy the
        // old 2-arg service registered above.
        configurator.Builder.WithRegistration<IMongoReadClient<Guid, TChangeTrackingEntity>,
            MongoReadClient<Guid, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity, TChangeTrackingEntity>,
            MongoChangeTrackingReadRepository<TKey, TEntity, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<IDefaultEntityOrderByProvider<Guid, TChangeTrackingEntity>,
            DefaultEntityOrderByProviderModified<Guid, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<IEntityQueryOrderByHandler<Guid, TChangeTrackingEntity>,
            DefaultEntityQueryOrderByHandler<Guid, TChangeTrackingEntity>>(false);

        configurator.Builder.WithRegistration<
            IEntitySearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TKey>>,
            MongoFullTextSearchHandler<Guid, TChangeTrackingEntity, ChangeTrackingSearchRequest<TKey>>>(false);

        if (configurator.Builder.IsTenantEntity)
        {
            configurator.Builder.WithRegistration<IMongoEntityConfigurationTenantTransformService<Guid, TChangeTrackingEntity>,
                MongoEntityConfigurationTenantTransformService<Guid, TChangeTrackingEntity>>(false);

            configurator.Builder.WithRegistration<IMongoConfigurationAllShardsProvider<Guid, TChangeTrackingEntity>,
                MongoConfigurationAllShardsProvider<Guid, TChangeTrackingEntity>>(false);

            configurator.Builder.WithRegistration<IConfigureCollection<Guid, TChangeTrackingEntity>,
                MongoConfigureShardedCollection<Guid, TChangeTrackingEntity>>(false);

            configurator.Builder.WithRegistration<IConfigureCollection,
                MongoConfigureShardedCollection<Guid, TChangeTrackingEntity>>(false, true);

            configurator.Builder.WithRegistrationInstance<IMongoEntityDefaultConfiguration<Guid, TChangeTrackingEntity>>(
                new MongoEntityDefaultConfiguration<Guid, TChangeTrackingEntity>(mongoDbEntityBuilder.CollectionName + "_ChangeTracking",
                    mongoDbEntityBuilder.Database,
                    mongoDbEntityBuilder.AggregateOption,
                    mongoDbEntityBuilder.ShardMode));

            configurator.Builder.WithRegistration<IMongoEntityConfiguration<Guid, TChangeTrackingEntity>,
                MongoTenantEntityConfiguration<Guid, TChangeTrackingEntity>>(false);
        }
        else
        {
            configurator.Builder.WithRegistration<IConfigureCollection<Guid, TChangeTrackingEntity>,
                MongoConfigureCollection<Guid, TChangeTrackingEntity>>();

            configurator.Builder.WithRegistration<IConfigureCollection, MongoConfigureCollection<Guid,
                TChangeTrackingEntity>>(false, true);

            configurator.Builder.WithRegistrationInstance<IMongoEntityConfiguration<Guid, TChangeTrackingEntity>>(
                new MongoEntityConfiguration<Guid, TChangeTrackingEntity>(mongoDbEntityBuilder.CollectionName + "_ChangeTracking",
                    mongoDbEntityBuilder.Database,
                    mongoDbEntityBuilder.AggregateOption,
                    mongoDbEntityBuilder.ShardMode));
        }

        configurator.WithDomainEventEntityAddedSubscriber<ChangeTrackingAddedDomainEventHandler<TKey, TEntity>>();
        configurator.WithDomainEventEntityUpdatedSubscriber<ChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>>();
        configurator.WithDomainEventEntityDeletedSubscriber<ChangeTrackingDeleteDomainEventHandler<TKey, TEntity>>();

        using var changeTrackingConfigurator =
            new MongoChangeTrackingConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TChangeTrackingEntity>(configurator.Builder);

        configure(changeTrackingConfigurator);

        return configurator;
    }
}
