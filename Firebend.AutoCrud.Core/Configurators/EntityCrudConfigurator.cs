using System;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Configurators
{
    public class EntityCrudConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
    {
        public EntityCrudConfigurator(TBuilder builder) : base(builder)
        {
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCrud<TSearch>()
            where TSearch : EntitySearchRequest
        {
            WithCreate();
            WithRead();
            WithUpdate();
            WithDelete();
            WithSearch<TSearch>();

            return this;
        }

        /// <summary>
        /// Enables Create, Read, Update, Delete, and Search actions for an entity
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
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCrud()
        {
            WithCreate();
            WithRead();
            WithUpdate();
            WithDelete();
            WithSearch(Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.SearchRequestType), Builder.SearchRequestType);

            return this;
        }

        /// <summary>
        /// Enables Create actions for an entity by providing a custom service
        /// </summary>
        /// <param name="serviceType">The type of the service to use</param>
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
        ///                      .WithCreate(typeof(WeatherForecastsService))
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate(Type serviceType)
        {
            Builder.WithRegistration<IEntityCreateService<TKey, TEntity>>(serviceType);

            return this;
        }

        /// <summary>
        /// Enables Create actions for an entity by providing a custom service
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
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
        ///                      .WithCreate<WeatherForecastsService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate<TService>() => WithCreate(typeof(TService));

        /// <summary>
        /// Enables Create actions for an entity with a default service
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
        ///                  .AddCrud(x => x
        ///                      .WithCreate()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithCreate()
        {
            var serviceType = Builder.CreateType;

            return WithCreate(serviceType);
        }

        /// <summary>
        /// Enables Delete actions for an entity by providing a custom service
        /// </summary>
        /// <param name="serviceType">The type of the service to use</param>
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
        ///                      .WithDelete(typeof(WeatherForecastsService))
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete(Type serviceType)
        {
            Builder.WithRegistration<IEntityDeleteService<TKey, TEntity>>(serviceType);

            return this;
        }

        /// <summary>
        /// Enables Delete actions for an entity by providing a custom service
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
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
        ///                      .WithDelete<WeatherForecastsService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete<TService>() => WithDelete(typeof(TService));

        /// <summary>
        /// Enables Delete actions for an entity with a default service
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
        ///                  .AddCrud(x => x
        ///                      .WithDelete()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithDelete()
        {
            var serviceType = Builder.DeleteType;

            return WithDelete(serviceType);
        }

        /// <summary>
        /// Enables Read actions for an entity by providing a custom service
        /// </summary>
        /// <param name="serviceType">The type of the service to use</param>
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
        ///                      .WithRead(typeof(WeatherForecastsService))
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead(Type serviceType)
        {
            Builder.WithRegistration<IEntityReadService<TKey, TEntity>>(serviceType);

            return this;
        }

        /// <summary>
        /// Enables Read actions for an entity by providing a custom service
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
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
        ///                      .WithRead<WeatherForecastsService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead<TService>() => WithRead(typeof(TService));

        /// <summary>
        /// Enables Read actions for an entity with a default service
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
        ///                  .AddCrud(x => x
        ///                      .WithRead()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithRead()
        {
            var serviceType = Builder.ReadType;

            return WithRead(serviceType);
        }

        /// <summary>
        /// Enables search for an entity via the <code>GET /{entity}</code> and <code>GET /{entity}/all</code> endpoints by providing a custom service and custom search fields
        /// </summary>
        /// <param name="serviceType">The type of the service to use</param>
        /// <param name="searchType">The type to use for search, must extend <code>EntitySearchRequest</code></param>
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
        ///                      .WithSearch(typeof(WeatherForecastService), typeof(WeatherForecastSearchService))
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        /// See <see cref="EntitySearchRequest" /> for building custom search fields
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch(Type serviceType, Type searchType)
        {
            Builder.SearchRequestType = searchType;

            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType));

            return this;
        }

        /// <summary>
        /// Enables search for an entity via the <code>GET /{entity}</code> and <code>GET /{entity}/all</code> endpoints by providing a custom service and custom search fields
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
        /// <typeparam name="TSearch">The type to use for search, must extend <code>EntitySearchRequest</code></typeparam>
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
        ///                      .WithSearch<WeatherForecastService, WeatherForecastSearchService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        /// See <see cref="EntitySearchRequest" /> for building custom search fields
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TService, TSearch>()
            where TSearch : EntitySearchRequest => WithSearch(typeof(TService), typeof(TSearch));

        /// <summary>
        /// Enables search for an entity via the <code>GET /{entity}</code> and <code>GET /{entity}/all</code> endpoints by providing custom search fields
        /// </summary>
        /// <typeparam name="TSearch">The type to use for search, must extend <code>EntitySearchRequest</code></typeparam>
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
        ///                      .WithSearch<WeatherForecastSearchService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        /// See <see cref="EntitySearchRequest" /> for building custom search fields
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);

            var serviceType = Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            return WithSearch(serviceType, searchType);
        }

        /// <summary>
        /// Enables search for an entity via the <code>GET /{entity}</code> and <code>GET /{entity}/all</code> endpoints by providing a callback function
        /// </summary>
        /// <typeparam name="TSearch">The type to use for search, must extend <code>EntitySearchRequest</code></typeparam>
        /// <param name="expression">A callback function for performing a search, return a callback for filtering matching objects</param>
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
        ///                      .WithSearch(search => {
        ///                           if (!string.IsNullOrWhiteSpace(search?.Search)) {
        ///                                return p => p.Summary.Contains(search?.Search);
        ///                           }
        ///                           return null;
        ///                      })
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch() => WithSearch<EntitySearchRequest>();

        /// <summary>
        /// Enables Update actions for an entity by providing a custom service
        /// </summary>
        /// <param name="serviceType">The type of the service to use</param>
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
        ///                      .WithUpdate(typeof(WeatherForecastsService))
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate(Type serviceType)
        {
            Builder.WithRegistration<IEntityUpdateService<TKey, TEntity>>(serviceType);

            return this;
        }

        /// <summary>
        /// Enables Update actions for an entity by providing a custom service
        /// </summary>
        /// <typeparam name="TService">The type of the service to use</typeparam>
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
        ///                      .WithUpdate<WeatherForecastsService>()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate<TService>() => WithUpdate(typeof(TService));

        /// <summary>
        /// Enables Update actions for an entity with a default service
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
        ///                  .AddCrud(x => x
        ///                      .WithUpdate()
        ///                      // ... finish configuring CRUD for this entity
        ///                   )
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithUpdate()
        {
            var serviceType = Builder.UpdateType;

            return WithUpdate(serviceType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithQueryCustomizer(Type type)
        {
            var service = typeof(IEntityQueryCustomizer<,,>).MakeGenericType(Builder.EntityType,
                Builder.EntityType,
                Builder.SearchRequestType);

            Builder.WithRegistration(service, type);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity>WithQueryCustomizer<TCustomizer>()
            => WithQueryCustomizer(typeof(TCustomizer));

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithQueryCustomizer<TSearch>(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
            where TSearch : EntitySearchRequest
        {
            Builder.SearchRequestType = typeof(TSearch);
            var instance = new DefaultEntityQueryCustomizer<TKey, TEntity, TSearch>(func);
            Builder.WithRegistrationInstance<IEntityQueryCustomizer<TKey, TEntity, TSearch>>(instance);

            return this;
        }
    }
}
