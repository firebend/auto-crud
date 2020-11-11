using System;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces
{
    public interface IChangeTrackingDbContextProvider<TEntityKey, TEntity> :
        IDbContextProvider<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {

    }
}
