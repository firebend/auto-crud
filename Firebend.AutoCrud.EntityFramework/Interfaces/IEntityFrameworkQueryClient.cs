using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkQueryClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<TEntity> GetByKeyAsync(TKey key,
            bool asNoTracking,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(
            bool asNoTracking,
            CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity>> PageAsync(
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = true,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TOut>> PageAsync<TOut>(
            Expression<Func<TEntity, TOut>> projection,
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = false,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            bool asNoTracking = true,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            string search,
            Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default);
    }
}
