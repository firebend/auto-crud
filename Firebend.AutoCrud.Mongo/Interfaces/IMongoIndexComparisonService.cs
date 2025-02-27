using MongoDB.Bson;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoIndexComparisonService
{
    public bool DoesIndexMatch<TEntity>(IMongoCollection<TEntity> collection, BsonDocument existingIndexBson, CreateIndexModel<TEntity> definition);

    public bool EnsureUnique<TEntity>(IMongoCollection<TEntity> collection, CreateIndexModel<TEntity>[] definitions);
}
