using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public class MongoEntityCrudGenerator : EntityCrudGenerator
    {
        public MongoEntityCrudGenerator(IServiceCollection collection, string connectionString) : base(collection)
        {
            collection.ConfigureMongoDb(connectionString, true, new MongoDbConfigurator());
        }
    }
}