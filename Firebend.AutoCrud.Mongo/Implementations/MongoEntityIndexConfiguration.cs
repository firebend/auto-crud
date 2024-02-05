using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class MongoEntityIndexConfiguration<TKey, TEntity> : MongoEntityConfiguration<TKey, TEntity>, IMongoEntityIndexConfiguration<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public string Locale { get; set; }
    public string ShardKey { get; set; }

    public MongoEntityIndexConfiguration(string collectionName,
        string databaseName,
        AggregateOptions aggregateOption,
        string shardKey,
        MongoTenantShardMode tenantShardMode = MongoTenantShardMode.Unknown,
        string locale = "en") : base(collectionName, databaseName, aggregateOption, tenantShardMode)
    {
        ShardKey = shardKey;
        Locale = locale;
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
