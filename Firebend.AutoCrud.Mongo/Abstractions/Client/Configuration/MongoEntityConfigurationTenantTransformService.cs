using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public abstract class MongoEntityConfigurationTenantTransformService<TKey, TEntity> : IMongoEntityConfigurationTenantTransformService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public string GetCollection(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration, string shardKey)
            => string.IsNullOrWhiteSpace(shardKey) || configuration.ShardMode != MongoTenantShardMode.Collection
                ? configuration.CollectionName
                : $"{shardKey}_{configuration.CollectionName}";

        public string GetDatabase(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration, string shardKey)
            => string.IsNullOrWhiteSpace(shardKey) || configuration.ShardMode != MongoTenantShardMode.Database
                ? configuration.DatabaseName
                : $"{shardKey}_{configuration.DatabaseName}";
    }
}
