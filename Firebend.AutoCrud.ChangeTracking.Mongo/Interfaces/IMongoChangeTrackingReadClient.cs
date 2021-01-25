using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Interfaces
{
    public interface IMongoChangeTrackingReadClient<TEntityKey, TEntity> : IMongoReadClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {

    }
}
