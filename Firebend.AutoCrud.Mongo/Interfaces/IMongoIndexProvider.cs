using System.Collections.Generic;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoIndexProvider<TEntity>
    {
        IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder);
    }
}
