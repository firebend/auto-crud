using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoEntityIndexConfiguration<TKey, TEntity> : IMongoEntityConfiguration<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public string Locale { get; }
    public string ShardKey { get; }
}
