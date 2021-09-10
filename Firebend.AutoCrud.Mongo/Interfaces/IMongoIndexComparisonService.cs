using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoIndexComparisonService
    {
        bool DoesIndexMatch<TEntity>(IMongoCollection<TEntity> collectionName, BsonDocument existingIndexBson, CreateIndexModel<TEntity> definition);

        //todo: ensure only one text index
        bool EnsureUnique<TEntity>(IEnumerable<CreateIndexModel<TEntity>> definitions);
    }
}
