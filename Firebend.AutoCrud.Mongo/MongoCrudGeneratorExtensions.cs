using System;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public static class MongoCrudGeneratorExtensions
    {
        public static MongoEntityCrudGenerator UsingMongoCrud(this IServiceCollection serviceCollection,
            string connectionString) => new MongoEntityCrudGenerator(serviceCollection, connectionString);


        // <summary>
        // Generates and adds configured Mongo entities to application's service collection
        // </summary>
        // <param name="connectionString">The connection string to your mongo database</param>
        // <param name="configure">A callback function that allows configuring entities</param>
        // <example>
        // <code>
        // public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        //  .ConfigureWebHostDefaults(webbuilder => { webBuilder.UseStartup<Startup>(); })
        //  .ConfigureServices((hostContext, services) => {
        //      services.UsingMongoCrud("mongodb://localhost:27017", mongo => {
        //          // ... configure entities
        //      });
        //  })
        //  // ...
        // </code>
        // </example>
        // See <see cref="MongoEntityCrudGenerator.AddEntity"/> for configuring entities
        public static IServiceCollection UsingMongoCrud(this IServiceCollection serviceCollection,
            string connectionString,
            Action<MongoEntityCrudGenerator> configure)
        {
            var mongo = serviceCollection.UsingMongoCrud(connectionString);
            configure(mongo);
            return mongo.Generate();
        }
    }
}
