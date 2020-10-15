using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Implementations.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsDomainEvents
    {
        public static TBuilder WithDomainEventPublisher<TBuilder>(this TBuilder builder, Type type)
            where TBuilder : EntityCrudBuilder
        {
            builder.WithRegistration(typeof(IEntityDomainEventPublisher), type, typeof(IEntityDomainEventPublisher));

            return builder;
        }

        public static TBuilder WithDomainEventPublisher<TBuilder, TPublisher>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDomainEventPublisher(typeof(TPublisher));
        }

        public static TBuilder WithDomainEventPublisherServiceProvider<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDomainEventPublisher(typeof(ServiceProviderDomainEventPublisher));
        }

        public static TBuilder WithDomainEventEntityAddedSubscriber<TBuilder>(this TBuilder builder, Type type)
            where TBuilder : EntityCrudBuilder
        {
            builder.WithRegistration(typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(builder.EntityType),
                type,
                typeof(IEntityAddedDomainEventSubscriber<>).MakeGenericType(builder.EntityType));

            return builder;
        }

        public static TBuilder WithDomainEventEntityAddedSubscriber<TBuilder, TSubscriber>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDomainEventEntityAddedSubscriber(typeof(TSubscriber));
        }

        public static TBuilder WithDomainEventEntityUpdatedSubscriber<TBuilder>(this TBuilder builder, Type type)
            where TBuilder : EntityCrudBuilder
        {
            builder.WithRegistration(typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(builder.EntityType),
                type,
                typeof(IEntityUpdatedDomainEventSubscriber<>).MakeGenericType(builder.EntityType));

            return builder;
        }

        public static TBuilder WithDomainEventEntityUpdatedSubscriber<TBuilder, TSubscriber>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDomainEventEntityUpdatedSubscriber(typeof(TSubscriber));
        }

        public static TBuilder WithDomainEventEntityDeletedSubscriber<TBuilder>(this TBuilder builder, Type type)
            where TBuilder : EntityCrudBuilder
        {
            builder.WithRegistration(typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(builder.EntityType),
                type,
                typeof(IEntityDeletedDomainEventSubscriber<>).MakeGenericType(builder.EntityType));

            return builder;
        }

        public static TBuilder WithDomainEventEntityDeletedSubscriber<TBuilder, TSubscriber>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDomainEventEntityDeletedSubscriber(typeof(TSubscriber));
        }
    }
}