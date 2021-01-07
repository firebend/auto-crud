using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class MongoEntityConfiguration<TKey, TEntity> : IMongoEntityConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        protected MongoEntityConfiguration()
        {

        }

        public MongoEntityConfiguration(string collectionName,
            string databaseName,
            MongoTenantShardMode tenantShardMode = MongoTenantShardMode.Unknown)
        {
            CollectionName = collectionName;
            DatabaseName = databaseName;
            ShardMode = tenantShardMode;
        }

        public string CollectionName { get; }

        public string DatabaseName { get; }

        public MongoTenantShardMode ShardMode { get; }
    }

    public class MongoTenantEntityConfiguration<TKey, TEntity> : IMongoEntityConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public MongoTenantEntityConfiguration(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration,
            IMongoEntityConfigurationTenantTransformService<TKey, TEntity> transformService,
            IMongoShardKeyProvider shardKeyProvider) : this(configuration, transformService, shardKeyProvider.GetShardKey())
        {

        }

        public MongoTenantEntityConfiguration(IMongoEntityDefaultConfiguration<TKey, TEntity> configuration,
            IMongoEntityConfigurationTenantTransformService<TKey, TEntity> transformService,
            string shardKey)
        {
            CollectionName = transformService.GetCollection(configuration, shardKey);
            DatabaseName = transformService.GetDatabase(configuration, shardKey);
            ShardMode = configuration.ShardMode;
        }

        public string CollectionName { get; }
        public string DatabaseName { get; }
        public MongoTenantShardMode ShardMode { get; }

    }
}
