using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Helpers;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class ModifiedFullTextIndexProvider<TKey, TEntity> : IMongoIndexProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IModifiedEntity
    {
        public IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder, IMongoEntityIndexConfiguration<TKey, TEntity> configuration)
        {
            yield return MongoIndexProviderHelpers.DateTimeOffset(builder, configuration.Locale);
            yield return MongoIndexProviderHelpers.FullText(builder);
        }
    }
}
