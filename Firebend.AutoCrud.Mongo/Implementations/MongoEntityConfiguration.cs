using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations;

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

    public ReadPreferenceMode? ReadPreferenceMode { get; }
}
