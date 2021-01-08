using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.DomainEvents.MassTransit.Extensions
{
    public static class DomainEventConfiguratorExtensions
    {
        /// <summary>
        /// Add domain event handling with MassTransit to the entity
        /// </summary>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddDomainEvents(events => events
        ///         .WithEfChangeTracking()
        ///         .WithMassTransit()
        ///      )
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers(true, true)
        ///          .WithIoControllers())
        /// </code>
        /// </example>
        public static DomainEventsConfigurator<TBuilder, TKey, TEntity> WithMassTransit<TBuilder, TKey, TEntity>(
            this DomainEventsConfigurator<TBuilder, TKey, TEntity> source)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            source.WithDomainEventPublisher<MassTransitDomainEventPublisher>();

            return source;
        }
    }
}
