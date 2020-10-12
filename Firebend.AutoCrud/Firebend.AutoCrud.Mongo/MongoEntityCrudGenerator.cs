using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Mongo
{
    public class GenericMongoEntityCrudGenerator<TBuilder> : EntityCrudGenerator<TBuilder> where TBuilder : EntityBuilder, new()
    {
    }

    public class MongoEntityCrudGenerator : EntityCrudGenerator<MongoDbEntityBuilder>
    {
        public MongoEntityCrudGenerator(IServiceCollection collection, string connectionString)
        {
            collection.ConfigureMongoDb(connectionString, true, new MongoDbConfigurator());
        }
    }
}