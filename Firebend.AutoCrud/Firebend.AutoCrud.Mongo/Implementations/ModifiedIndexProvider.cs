using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Helpers;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class ModifiedIndexProvider<TEntity>: IMongoIndexProvider<TEntity>
        where TEntity: IModifiedEntity
    {
        public IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder)
        {
            yield return MongoIndexProviderHelpers.DateTimeOffset(builder);
        }
    }
}