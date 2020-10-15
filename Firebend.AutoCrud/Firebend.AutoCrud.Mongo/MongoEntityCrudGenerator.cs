#region

using Firebend.AutoCrud.Generator.Implementations;
using Firebend.AutoCrud.Mongo.Configuration;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Firebend.AutoCrud.Mongo
{
    public class MongoEntityCrudGenerator : EntityCrudGenerator<MongoDbEntityBuilder>
    {
        public MongoEntityCrudGenerator(IServiceCollection collection, string connectionString) : base(collection)
        {
            collection.ConfigureMongoDb(connectionString, true, new MongoDbConfigurator());
        }
    }
}