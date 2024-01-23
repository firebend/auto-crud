using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoIndexProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder, IMongoEntityIndexConfiguration<TKey, TEntity> configuration);
}
