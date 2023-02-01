using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

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
            AggregateOptions aggregateOption,
            MongoTenantShardMode tenantShardMode = MongoTenantShardMode.Unknown)
        {
            CollectionName = collectionName;
            DatabaseName = databaseName;
            AggregateOption = aggregateOption;
            ShardMode = tenantShardMode;
        }

        public string CollectionName { get; }

        public string DatabaseName { get; }

        public AggregateOptions AggregateOption { get; set; }

        public MongoTenantShardMode ShardMode { get; }
    }

    public class MongoEntityIndexConfiguration<TKey, TEntity> : MongoEntityConfiguration<TKey, TEntity>, IMongoEntityIndexConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public string ShardKey { get; set; }

        public MongoEntityIndexConfiguration(string collectionName,
            string databaseName,
            AggregateOptions aggregateOption,
            string shardKey,
            MongoTenantShardMode tenantShardMode = MongoTenantShardMode.Unknown) : base(collectionName, databaseName, aggregateOption, tenantShardMode)
        {
            ShardKey = shardKey;
        }

        public static MongoEntityIndexConfiguration<TKey, TEntity> FromConfiguration(IMongoEntityConfiguration<TKey, TEntity> configuration)
        {
            return new MongoEntityIndexConfiguration<TKey, TEntity>(
                configuration.CollectionName,
                configuration.DatabaseName,
                configuration.AggregateOption,
                null,
                configuration.ShardMode);
        }
    }

    public class MongoTenantEntityConfiguration<TKey, TEntity> : IMongoEntityConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public MongoTenantEntityConfiguration(IMongoEntityConfigurationTenantTransformService<TKey, TEntity> transformService,
            IMongoShardKeyProvider shardKeyProvider,
            IMongoEntityDefaultConfiguration<TKey, TEntity> configuration)
        {
            var shardKey = shardKeyProvider.GetShardKey();
            CollectionName = transformService.GetCollection(configuration, shardKey);
            DatabaseName = transformService.GetDatabase(configuration, shardKey);
            AggregateOption = configuration.AggregateOption;
            ShardMode = configuration.ShardMode;
        }

        public string CollectionName { get; }

        public string DatabaseName { get; }

        public AggregateOptions AggregateOption { get; set; }

        public MongoTenantShardMode ShardMode { get; }
    }

    public class MongoEntityDefaultConfiguration<TKey, TEntity> : IMongoEntityDefaultConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        protected MongoEntityDefaultConfiguration()
        {

        }

        public MongoEntityDefaultConfiguration(string collectionName,
            string databaseName,
            AggregateOptions aggregateOption,
            MongoTenantShardMode tenantShardMode = MongoTenantShardMode.Unknown)
        {
            CollectionName = collectionName;
            DatabaseName = databaseName;
            AggregateOption = aggregateOption;
            ShardMode = tenantShardMode;
        }

        public string CollectionName { get; }

        public string DatabaseName { get; }

        public AggregateOptions AggregateOption { get; set; }

        public MongoTenantShardMode ShardMode { get; }
    }
}
