#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityDeleteService<in TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default);
    }
}