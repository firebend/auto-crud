#region

using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

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