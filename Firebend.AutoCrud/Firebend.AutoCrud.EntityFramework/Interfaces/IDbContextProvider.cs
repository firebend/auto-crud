#region

using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IDbContextProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        IDbContext GetDbContext();
    }
}