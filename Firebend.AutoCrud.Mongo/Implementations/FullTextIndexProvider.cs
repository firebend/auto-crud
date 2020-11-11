using System.Collections.Generic;
using Firebend.AutoCrud.Mongo.Helpers;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class FullTextIndexProvider<TEntity> : IMongoIndexProvider<TEntity>
    {
        public IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder)
        {
            yield return MongoIndexProviderHelpers.FullText(builder);
        }
    }
}
