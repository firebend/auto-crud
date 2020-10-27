using System;
using Firebend.AutoCrud.ChangeTracking.Abstractions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework
{
    public static class Extensions
    {
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithEfChangeTracking<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> configurator)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
        {
            if (!(configurator.Builder is EntityFrameworkEntityBuilder<TKey, TEntity>))
            {
                throw new Exception($"Configuration Error! This builder is not a {nameof(EntityFrameworkEntityBuilder<Guid, FooEntity>)}");
            }

            configurator.Builder.WithRegistration<IChangeTrackingDbContextProvider<TKey, TEntity>,
                AbstractChangeTrackingDbContextProvider<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingDbContextProvider<TKey, TEntity>,
                AbstractChangeTrackingDbContextProvider<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingReadService<TKey, TEntity>,
                AbstractEntityFrameworkChangeTrackingReadService<TKey, TEntity>>();

            configurator.Builder.WithRegistration<IChangeTrackingService<TKey, TEntity>,
                AbstractEntityFrameworkChangeTrackingService<TKey, TEntity>>();

            configurator.WithDomainEventEntityAddedSubscriber<AbstractChangeTrackingAddedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityUpdatedSubscriber<AbstractChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>>();
            configurator.WithDomainEventEntityDeletedSubscriber<AbstractChangeTrackingDeleteDomainEventHandler<TKey, TEntity>>();
            
            return configurator;
        }
    }
}