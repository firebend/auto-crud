using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Implementations
{
    public class MongoChangeTrackingEntityConfiguration<TEntityKey, TEntity> :
        IMongoEntityConfiguration<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        private readonly IMongoEntityConfiguration<TEntityKey, TEntity> _entityConfiguration;

        public MongoChangeTrackingEntityConfiguration(IMongoEntityConfiguration<TEntityKey, TEntity> entityConfiguration)
        {
            _entityConfiguration = entityConfiguration;
        }

        public string CollectionName => $"{_entityConfiguration.CollectionName}_ChangeTracking";

        public string DatabaseName => _entityConfiguration.DatabaseName;
    }
}