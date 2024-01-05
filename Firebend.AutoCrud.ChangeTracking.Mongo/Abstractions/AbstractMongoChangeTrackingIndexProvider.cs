using System;
using System.Collections.Generic;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Helpers;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingIndexProvider<TEntityKey, TEntity> :
        IMongoIndexProvider<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        public IEnumerable<CreateIndexModel<ChangeTrackingEntity<TEntityKey, TEntity>>> GetIndexes(
            IndexKeysDefinitionBuilder<ChangeTrackingEntity<TEntityKey, TEntity>> builder,
            IMongoEntityIndexConfiguration<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> configuration)
        {
            yield return new CreateIndexModel<ChangeTrackingEntity<TEntityKey, TEntity>>(
                builder.Ascending(f => f.EntityId),
                new CreateIndexOptions { Name = "changeTrackingEntityId" });

            yield return MongoIndexProviderHelpers.FullText(builder, configuration.Locale);

            yield return MongoIndexProviderHelpers.DateTimeOffset(builder, configuration.Locale);
        }
    }
}
