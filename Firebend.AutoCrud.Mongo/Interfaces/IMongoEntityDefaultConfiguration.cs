using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoEntityDefaultConfiguration<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    public string CollectionName { get; }

    public string DatabaseName { get; }

    public AggregateOptions AggregateOption { get; set; }

    public MongoTenantShardMode ShardMode { get; }
}
