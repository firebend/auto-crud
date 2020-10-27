using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.DomainEvents.MassTransit.DomainEventHandlers;
using Firebend.AutoCrud.DomainEvents.MassTransit.Interfaces;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions
{
    public static class DomainEventConfiguratorExtensions
    {
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithMassTransit<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> source)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            source.WithDomainEventPublisher<MassTransitDomainEventPublisher>();
            
            // if (source.Builder.HasRegistration<IEntityAddedDomainEventSubscriber<TEntity>>())
            // {
            //     source.Builder.WithRegistration<
            //         IMassTransitDomainEventHandler<EntityAddedDomainEvent<TEntity>>,
            //         MassTransitEntityAddedDomainEventHandler<TEntity>>();
            // }
            //
            // if (source.Builder.HasRegistration<IEntityUpdatedDomainEventSubscriber<TEntity>>())
            // {
            //     source.Builder.WithRegistration<
            //         IMassTransitDomainEventHandler<EntityUpdatedDomainEvent<TEntity>>,
            //         MassTransitEntityUpdatedDomainEventHandler<TEntity>>();
            // }
            //
            // if (source.Builder.HasRegistration<IEntityDeletedDomainEventSubscriber<TEntity>>())
            // {
            //     source.Builder.WithRegistration<
            //         IMassTransitDomainEventHandler<EntityDeletedDomainEvent<TEntity>>,
            //         MassTransitEntityDeletedDomainEventHandler<TEntity>>();
            // }

            return source;
        }
    }
}