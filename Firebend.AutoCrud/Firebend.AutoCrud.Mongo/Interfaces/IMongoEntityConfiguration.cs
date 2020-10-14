using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoEntityConfiguration<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public string CollectionName { get; }

        public string DatabaseName { get; }
    }
}