using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class DefaultIndexProvider<TEntity> : IMongoIndexProvider<TEntity>
    {
        public IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder)
        {
            return Enumerable.Empty<CreateIndexModel<TEntity>>();
        }
    }
}