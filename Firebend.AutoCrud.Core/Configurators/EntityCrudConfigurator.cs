using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Implementations;
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

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy(Type type)
        {
            Builder.WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>>(type);

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy<T>() => WithOrderBy(typeof(T));

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithOrderBy(Expression<Func<TEntity, object>> expression, bool isAscending = true)
        {
            var instance = new DefaultEntityDefaultOrderByProvider<TKey, TEntity>
            {
                OrderBy = (
                    expression,
                    isAscending
                )
            };

            Builder.WithRegistrationInstance<IEntityDefaultOrderByProvider<TKey, TEntity>>(instance);

            return this;
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

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch(Type serviceType, Type searchType)
        {
            Builder.SearchRequestType = searchType;

            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            Builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType));

            return this;
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TService, TSearch>()
            where TSearch : EntitySearchRequest => WithSearch(typeof(TService), typeof(TSearch));

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TSearch>()
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);

            var serviceType = Builder.SearchType.MakeGenericType(Builder.EntityKeyType, Builder.EntityType, searchType);

            return WithSearch(serviceType, searchType);
        }

        public EntityCrudConfigurator<TBuilder, TKey, TEntity> WithSearch<TSearch>(Func<TSearch, Expression<Func<TEntity, bool>>> expression)
            where TSearch : EntitySearchRequest
        {
            Builder.WithRegistrationInstance<ISearchExpressionProvider<TKey, TEntity, TSearch>>(
                new SearchExpressionProvider<TKey, TEntity, TSearch>(expression));

            return WithSearch<TSearch>();
        }

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
    }
}
