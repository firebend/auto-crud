using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Implementations.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

namespace Firebend.AutoCrud.Core.Configurators
{
    public class DomainEventsConfigurator<TBuilder> : BuilderConfigurator<TBuilder> where TBuilder : EntityBuilder
    {
        public DomainEventsConfigurator(TBuilder builder) : base(builder)
        {
        }
        
        public DomainEventsConfigurator<TBuilder> WithDomainEventPublisher(Type type)
        {
            Builder.WithRegistration(typeof(IEntityDomainEventPublisher), type, typeof(IEntityDomainEventPublisher));

            return this;
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventPublisher<TPublisher>()
        {
            return WithDomainEventPublisher(typeof(TPublisher));
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventPublisherServiceProvider()
        {
            return WithDomainEventPublisher(typeof(ServiceProviderDomainEventPublisher));
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityAddedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType),
                type,
                typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType));

            return this;
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityAddedSubscriber<TSubscriber>()
        {
            return WithDomainEventEntityAddedSubscriber(typeof(TSubscriber));
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityUpdatedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType),
                type,
                typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType));

            return this;
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityUpdatedSubscriber<TSubscriber>()
        {
            return WithDomainEventEntityUpdatedSubscriber(typeof(TSubscriber));
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityDeletedSubscriber(Type type)
        {
            Builder.WithRegistration(typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType),
                type,
                typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(Builder.EntityType));

            return this;
        }

        public DomainEventsConfigurator<TBuilder> WithDomainEventEntityDeletedSubscriber<TSubscriber>()
        {
            return WithDomainEventEntityDeletedSubscriber(typeof(TSubscriber));
        }
    }
}