#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken);
    }
}