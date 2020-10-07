using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCreateService<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}