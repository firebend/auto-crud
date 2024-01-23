using System;
using Firebend.AutoCrud.Core.Implementations.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.Mongo;

public static class MongoCrudGeneratorExtensions
{
    /// <summary>
    /// Generates and adds configured Mongo entities to application's service collection
    /// </summary>
    /// <param name="connectionString">The connection string to your mongo database</param>
    /// <param name="enableLogging">True if a logger can be configured to log commands; otherwise, false.</param>
    /// <example>
    /// <code>
    /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
    ///  .ConfigureServices((hostContext, services) => {
    ///      services.UsingMongoCrud("mongodb://localhost:27017");
    ///  })
    ///  // ...
    /// </code>
    /// </example>
    public static MongoEntityCrudGenerator UsingMongoCrud(this IServiceCollection serviceCollection) => new(serviceCollection);


    /// <summary>
    /// Generates and adds configured Mongo entities to application's service collection
    /// </summary>
    /// <param name="connectionString">The connection string to your mongo database</param>
    /// <param name="enableLogging">True if a logger can be configured to log commands; otherwise, false.</param>
    /// <param name="configure">A callback function that allows configuring entities</param>
    /// <example>
    /// <code>
    /// public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    ///  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
    ///  .ConfigureServices((hostContext, services) => {
    ///      services.UsingMongoCrud("mongodb://localhost:27017", mongo => {
    ///          // ... configure entities
    ///      });
    ///  })
    ///  // ...
    /// </code>
    /// </example>
    /// See <see cref="MongoEntityCrudGenerator.AddEntity{TKey,TEntity}"/> for configuring entities
    public static IServiceCollection UsingMongoCrud(this IServiceCollection serviceCollection,
        Action<MongoEntityCrudGenerator> configure)
    {
        serviceCollection.TryAddScoped<IMongoIndexComparisonService, MongoIndexComparisonService>();
        serviceCollection.TryAddSingleton<IMemoizer>(Memoizer.Instance);

        using var mongo = serviceCollection.UsingMongoCrud();
        configure(mongo);
        return mongo.Generate();
    }
}
