using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityBuilderConfiguratorExtensions
    {
        /// <summary>
        /// Adds autocrud configuration for an entity
        /// </summary>
        /// <param name="configure">A callback allowing further configuration of the autocrud settings for an entity</param>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingMongoCrud("mongodb://localhost:27017", mongo => {
        ///          mongo.AddEntity<Guid, WeatherForecast>(forecast =>
        ///              forecast.WithDatabase("Samples")
        ///                  .WithCollection("WeatherForecasts")
        ///                  .WithFullTextSearch()
        ///                  .AddCrud(x => x
        ///                      .WithCrud()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        /// See <see cref="EntityCrudConfigurator"/> for additional configuration options available in the callback
        public static EntityCrudBuilder<TKey, TEntity> AddCrud<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
            configure(config);
            return builder;
        }


        /// <summary>
        /// Adds basic autocrud configuration for an entity
        /// </summary>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingMongoCrud("mongodb://localhost:27017", mongo => {
        ///          mongo.AddEntity<Guid, WeatherForecast>(forecast =>
        ///              forecast.WithDatabase("Samples")
        ///                  .WithCollection("WeatherForecasts")
        ///                  .WithFullTextSearch()
        ///                  .AddCrud()
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public static EntityCrudBuilder<TKey, TEntity> AddCrud<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey> => AddCrud(builder, crud => crud.WithCrud());

        /// <summary>
        /// Adds DomainEvents with MassTransit
        /// </summary>
        /// <param name="configure">A callback allowing further configuration of domain events settings for an entity</param>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingEfCrud(ef =>
        ///     {
        ///         ef.AddEntity<Guid, WeatherForecast>(forecast => 
        ///             forecast.WithDbContext<AppDbContext>()
        ///                 .AddCrud()
        ///                 .AddDomainEvents(events => events
        ///                     .WithEfChangeTracking()
        ///                     .WithMassTransit()
        ///                     .WithDomainEventEntityAddedSubscriber<DomainEventHandler>()
        ///                     .WithDomainEventEntityUpdatedSubscriber<DomainEventHandler>()
        ///                 )
        ///                 // ... finish configuring the entity
        ///             )
        ///         });
        ///     })
        /// </code>
        /// </example>
        public static EntityCrudBuilder<TKey, TEntity> AddDomainEvents<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
            Action<DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var config = new DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
            configure(config);
            return builder;
        }
    }
}
