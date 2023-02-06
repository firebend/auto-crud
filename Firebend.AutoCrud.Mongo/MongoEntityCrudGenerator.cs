using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoEntityCrudGenerator : EntityCrudGenerator
    {
        public MongoEntityCrudGenerator(IServiceCollection collection) : base(collection)
        {
            collection.ConfigureMongoDb(new MongoDbConfigurator());
        }

        /// <summary>
        /// Adds an entity configured by the callback to the application's service collection
        /// </summary>
        /// <typeparam name="TKey">A struct defining the primary key type for the entity</typeparam>
        /// <typeparam name="TEntity">The entity model class</typeparam>
        /// <param name="configure">A callback function that allows configuring the individual entity</param>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingMongoCrud(mongo => {
        ///          mongo.AddEntity<Guid, WeatherForecast>(forecast =>
        ///              forecast
        ///                  .WithConnectionString("mongodb://localhost:27017")
        ///                  .WithDefaultDatabase("Samples")
        ///                  .WithCollection("WeatherForecasts")
        ///                  // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        /// See <see cref="MongoDbEntityBuilder{TKey, TEntity}"/> extensions for options for configuring entities
        public MongoEntityCrudGenerator AddEntity<TKey, TEntity>(Action<MongoDbEntityBuilder<TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
        {
            var builder = new MongoDbEntityBuilder<TKey, TEntity>();
            configure(builder);
            Builders.Add(builder);
            return this;
        }

        /// <summary>
        /// Sets the default connection string to be used for mongo migrations
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <example>
        /// <code>
        /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        ///  .ConfigureServices((hostContext, services) => {
        ///      services.UsingMongoCrud(mongo => {
        ///          mongo
        ///              .WithDefaultConnectionString("mongodb://localhost:27017")
        ///              .AddEntity<Guid, WeatherForecast>(forecast =>
        ///                 forecast
        ///                      .WithConnectionString("mongodb://localhost:27017")
        ///                     .WithDefaultDatabase("Samples")
        ///                     .WithCollection("WeatherForecasts")
        ///                     // ... finish configuring the entity
        ///          )
        ///      });
        ///  })
        ///  // ...
        /// </code>
        /// </example>
        public MongoEntityCrudGenerator WithMigrationConnectionString(string connectionString)
        {
            ServiceCollection.TryAddSingleton<IMongoMigrationConnectionStringProvider>(
                new MongoMigrationConnectionStringProvider(connectionString));

            return this;
        }
    }
}
