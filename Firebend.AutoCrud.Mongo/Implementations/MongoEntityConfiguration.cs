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
        private string _collectionName;
        private string _databaseName;
        private readonly MongoEntityConfiguration<TKey, TEntity> _configuration;
        private readonly IMongoShardKeyProvider _shardKeyProvider;

        public string CollectionName => _collectionName ??= ApplyShard(_configuration.CollectionName, ShardMode == MongoTenantShardMode.Collection);
        public string DatabaseName => _databaseName ??= ApplyShard(_configuration.DatabaseName, ShardMode == MongoTenantShardMode.Database);
        public MongoTenantShardMode ShardMode { get; }

        private string ApplyShard(string value, bool shouldApply) => shouldApply ? $"{_shardKeyProvider.GetShardKey()}_{value}" : value;

        public MongoTenantEntityConfiguration(MongoEntityConfiguration<TKey, TEntity> configuration,
            IMongoShardKeyProvider shardKeyProvider)
        {
            _configuration = configuration;
            _shardKeyProvider = shardKeyProvider;
            ShardMode = configuration.ShardMode;
        }
    }
}
