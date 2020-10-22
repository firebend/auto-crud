using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingIndexClient<TEntityKey, TEntity> :
        MongoIndexClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct 
        where TEntity : class, IEntity<TEntityKey>
    {
        protected AbstractMongoChangeTrackingIndexClient(IMongoClient client,
            IMongoEntityConfiguration<TEntityKey, TEntity> entityConfiguration,
            ILogger<MongoIndexClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>> logger,
            IMongoIndexProvider<ChangeTrackingEntity<TEntityKey, TEntity>> indexProvider) : 
            base(client,
                new MongoChangeTrackingEntityConfiguration<TEntityKey, TEntity>(entityConfiguration),
                logger,
                indexProvider)
        {
        }
    }
}