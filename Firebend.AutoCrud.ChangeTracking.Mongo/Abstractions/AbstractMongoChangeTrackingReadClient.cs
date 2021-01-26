using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.ChangeTracking.Mongo.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public class AbstractMongoChangeTrackingReadClient<TEntityKey, TEntity> :
        MongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>,
        IMongoChangeTrackingReadClient<TEntityKey, TEntity>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        public AbstractMongoChangeTrackingReadClient(IMongoClient client,
            ILogger<MongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>> logger,
            IMongoEntityConfiguration<TEntityKey, TEntity> entityConfiguration,
            IEntityQueryOrderByHandler<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> orderByHandler) :
            base(client, logger, new MongoChangeTrackingEntityConfiguration<TEntityKey, TEntity>(entityConfiguration), orderByHandler)
        {
        }
    }
}
