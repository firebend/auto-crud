using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityReadService<in TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
