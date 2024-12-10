using System;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Caching;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;

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
    /// See <see cref="EntityCrudConfigurator{EntityCrudBuilder, TKey, TEntity}"/> for additional configuration options available in the callback
    public static EntityCrudBuilder<TKey, TEntity> AddCrud<TKey, TEntity>(this EntityCrudBuilder<TKey, TEntity> builder,
        Action<EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        using var config = new EntityCrudConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
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
    public static EntityCrudBuilder<TKey, TEntity> AddDomainEvents<TKey, TEntity>(
        this EntityCrudBuilder<TKey, TEntity> builder,
        Action<DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        using var domainEventsConfigurator =
            new DomainEventsConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity>(builder);
        configure(domainEventsConfigurator);
        return builder;
    }

    /// <summary>
    /// Adds caching to an entity using the default cache service. Must be called after <see cref="Firebend.AutoCrud.Core.Extensions.ServiceCollectionExtensions.WithEntityCaching<TCacheOptions>"/>.
    /// </summary>
    /// <param name="builder"><see cref="Firebend.AutoCrud.Core.Abstractions.Builders.EntityCrudBuilder<TKey,TEntity>"/></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns>The calling EntityCrudBuilder</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EntityCrudBuilder<TKey, TEntity> AddEntityCaching<TKey, TEntity>(
        this EntityCrudBuilder<TKey, TEntity> builder)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        if (builder.Services.All(x => x.ServiceType != typeof(IEntityCacheOptions)))
        {
            throw new InvalidOperationException(
                "WithEntityCaching must be called before AddEntityCaching.");
        }

        builder.Services.AddScoped<IEntityCacheService<TKey, TEntity>, DefaultEntityCacheService<TKey, TEntity>>();

        return builder;
    }

    /// <summary>
    /// Adds caching to an entity using the provided cache service. Must be called after <see cref="Firebend.AutoCrud.Core.Extensions.ServiceCollectionExtensions.WithEntityCaching<TCacheOptions>"/>.
    /// </summary>
    /// <param name="builder"><see cref="Firebend.AutoCrud.Core.Abstractions.Builders.EntityCrudBuilder<TKey,TEntity>"/></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TService">Entity Cache Service type to register</typeparam>
    /// <returns>The calling EntityCrudBuilder</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EntityCrudBuilder<TKey, TEntity> AddEntityCaching<TKey, TEntity, TService>(
        this EntityCrudBuilder<TKey, TEntity> builder)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TService : class, IEntityCacheService<TKey, TEntity>
    {
        if (builder.Services.All(x => x.ServiceType != typeof(IEntityCacheOptions)))
        {
            throw new InvalidOperationException(
                "WithEntityCaching must be called before AddEntityCaching.");
        }

        builder.Services.AddScoped<IEntityCacheService<TKey, TEntity>, TService>();

        return builder;
    }
}
