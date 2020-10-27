using System;
using Firebend.AutoCrud.ChangeTracking.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Mongo.Interfaces;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo
{
    public static class Extensions
    {
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithMongoChangeTracking<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> configurator)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
        {
            if (!(configurator.Builder is MongoDbEntityBuilder<TKey, TEntity>))
            {
                throw new Exception($"Configuration Error! This builder is not a {nameof(MongoDbEntityBuilder<Guid, FooEntity>)}");
            }
            
            configurator.Builder.WithRegistration<IMongoChangeTrackingCreateClient<TKey, TEntity>,
                AbstractMongoChangeTrackingCreateClient<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IMongoIndexClient<Guid, ChangeTrackingEntity<TKey, TEntity>>,
                AbstractMongoChangeTrackingIndexClient<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IMongoIndexProvider<ChangeTrackingEntity<TKey, TEntity>>,
                AbstractMongoChangeTrackingIndexProvider<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity>,
                AbstractMongoChangeTrackingReadRepository<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingService<TKey, TEntity>,
                AbstractMongoChangeTrackingService<TKey, TEntity>>();

            configurator.WithDomainEventEntityAddedSubscriber<AbstractChangeTrackingAddedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityUpdatedSubscriber<AbstractChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityDeletedSubscriber<AbstractChangeTrackingDeleteDomainEventHandler<TKey, TEntity>>();
            
            return configurator;
        }
    }
}