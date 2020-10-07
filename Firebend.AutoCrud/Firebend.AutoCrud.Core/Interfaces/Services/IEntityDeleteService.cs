using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityDeleteService<in TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default);
    }
}