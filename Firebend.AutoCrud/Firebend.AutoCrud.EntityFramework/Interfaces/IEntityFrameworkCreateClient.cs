#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    }
}