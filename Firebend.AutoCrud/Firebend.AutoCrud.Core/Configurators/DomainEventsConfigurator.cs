using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Implementations.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

namespace Firebend.AutoCrud.Core.Configurators
{
    public class DomainEventsConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct where TEntity : IEntity<TKey>
    {
        public DomainEventsConfigurator(TBuilder builder) : base(builder)
        {
        }
        
        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventPublisher(Type type)
        {
            Builder.WithRegistration(typeof(IEntityDomainEventPublisher), type, typeof(IEntityDomainEventPublisher));

            return this;
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventPublisher<TPublisher>()
            where TPublisher : IEntityDomainEventPublisher
        {
            return WithDomainEventPublisher(typeof(TPublisher));
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventPublisherServiceProvider()
        {
            return WithDomainEventPublisher(typeof(ServiceProviderDomainEventPublisher));
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityAddedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityAddedDomainEventSubscriber<TEntity>),
                type,
                typeof(IEntityAddedDomainEventSubscriber<TEntity>));

            return this;
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityAddedSubscriber<TSubscriber>()
            where TSubscriber : IEntityAddedDomainEventSubscriber<TEntity>
        {
            Builder.WithRegistration<IEntityAddedDomainEventSubscriber<TEntity>, TSubscriber>();
            return this;
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityUpdatedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityUpdatedDomainEventSubscriber<TEntity>),
                type,
                typeof(IEntityUpdatedDomainEventSubscriber<TEntity>));

            return this;
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityUpdatedSubscriber<TSubscriber>()
        {
            return WithDomainEventEntityUpdatedSubscriber(typeof(TSubscriber));
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityDeletedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityDeletedDomainEventSubscriber<TEntity>),
                type,
                typeof(IEntityDeletedDomainEventSubscriber<TEntity>));

            return this;
        }

        public DomainEventsConfigurator<TBuilder, TKey, TEntity> WithDomainEventEntityDeletedSubscriber<TSubscriber>()
        {
            return WithDomainEventEntityDeletedSubscriber(typeof(TSubscriber));
        }
    }
}