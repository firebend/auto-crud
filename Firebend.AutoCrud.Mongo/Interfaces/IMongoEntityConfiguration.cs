using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoEntityDefaultConfiguration<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public string CollectionName { get; }

        public string DatabaseName { get; }

        public MongoTenantShardMode ShardMode { get; }
    }

    public interface IMongoEntityConfiguration<TKey, TEntity> : IMongoEntityDefaultConfiguration<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
    }
}
