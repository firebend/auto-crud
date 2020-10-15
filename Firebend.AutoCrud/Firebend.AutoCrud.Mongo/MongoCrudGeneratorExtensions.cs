using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public static class MongoCrudGeneratorExtensions
    {
        public static MongoEntityCrudGenerator UsingMongoCrud(this IServiceCollection serviceCollection,
            string connectionString)
        {
            return new MongoEntityCrudGenerator(serviceCollection, connectionString);
        }
    }
}