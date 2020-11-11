using System.Collections.Generic;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingIndexProvider<TEntityKey, TEntity> :
        IMongoIndexProvider<ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        public IEnumerable<CreateIndexModel<ChangeTrackingEntity<TEntityKey, TEntity>>> GetIndexes(IndexKeysDefinitionBuilder<ChangeTrackingEntity<TEntityKey, TEntity>> builder)
        {
            yield return new CreateIndexModel<ChangeTrackingEntity<TEntityKey, TEntity>>(builder.Ascending(f => f.EntityId));
        }
    }
}
