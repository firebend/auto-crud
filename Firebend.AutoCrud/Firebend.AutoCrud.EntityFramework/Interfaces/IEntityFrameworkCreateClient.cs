using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkCreateClient<TKey, TEntity>
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    }
}