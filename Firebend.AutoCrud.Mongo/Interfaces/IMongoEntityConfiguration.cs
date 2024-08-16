using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoEntityConfiguration<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public string CollectionName { get; }

    public string DatabaseName { get; }

    public AggregateOptions AggregateOption { get; set; }

    public MongoTenantShardMode ShardMode { get; }

    public ReadPreferenceMode? ReadPreferenceMode { get; }
}
