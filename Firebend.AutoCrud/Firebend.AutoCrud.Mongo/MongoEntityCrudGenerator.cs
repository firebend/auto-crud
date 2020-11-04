using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
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

        public MongoEntityCrudGenerator AddEntity<TKey, TEntity>(Action<MongoDbEntityBuilder<TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
        {
            var builder = new MongoDbEntityBuilder<TKey, TEntity>();
            configure(builder);
            Builders.Add(builder);
            return this;
        }
    }
}