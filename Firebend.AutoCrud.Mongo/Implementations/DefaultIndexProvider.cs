using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class DefaultIndexProvider<TKey, TEntity>: IMongoIndexProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        public IEnumerable<CreateIndexModel<TEntity>> GetIndexes(IndexKeysDefinitionBuilder<TEntity> builder, IMongoEntityIndexConfiguration<TKey, TEntity> configuration) => Enumerable.Empty<CreateIndexModel<TEntity>>();
    }
}
