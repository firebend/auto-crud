using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Interfaces
{
    public interface IMongoChangeTrackingCreateClient<TEntityKey, TEntity> : IMongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {

    }
}
