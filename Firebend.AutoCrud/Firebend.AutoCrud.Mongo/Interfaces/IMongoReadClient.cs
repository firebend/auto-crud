using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoReadClient<TEntity, TKey>
        where TEntity: IEntity<TKey>
        where TKey : struct
    {
        Task<TEntity> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default);

        Task<List<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity>> PageAsync(
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = true,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TOut>> PageAsync<TOut>(
            Expression<Func<TEntity, TOut>> projection,
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = false,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
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