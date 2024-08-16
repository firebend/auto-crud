using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations;

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

    public ReadPreferenceMode? ReadPreferenceMode { get; }
}
