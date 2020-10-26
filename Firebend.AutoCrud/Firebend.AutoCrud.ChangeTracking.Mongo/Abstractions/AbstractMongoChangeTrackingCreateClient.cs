using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.ChangeTracking.Mongo.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingCreateClient<TEntityKey, TEntity> :
        MongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>,
        IMongoChangeTrackingCreateClient<TEntityKey, TEntity>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        public AbstractMongoChangeTrackingCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>> logger,
            IMongoEntityConfiguration<TEntityKey, TEntity> entityConfiguration) : 
            base(client,
                logger,
                new MongoChangeTrackingEntityConfiguration<TEntityKey, TEntity>(entityConfiguration),
                null,
                null)
        {
        }
    }
}