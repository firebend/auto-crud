using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityReadService<in TKey, TEntity> : IDisposable
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    }
}
