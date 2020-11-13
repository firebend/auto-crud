using System;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public static class MongoCrudGeneratorExtensions
    {
        public static MongoEntityCrudGenerator UsingMongoCrud(this IServiceCollection serviceCollection,
            string connectionString) => new MongoEntityCrudGenerator(serviceCollection, connectionString);

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
