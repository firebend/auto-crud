using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkQueryClient<TKey, TEntity> : IDisposable
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<TEntity> GetByKeyAsync(TKey key, bool track, CancellationToken cancellationToken);

        Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable, TSearchRequest searchRequest, CancellationToken cancellationToken = default)
            where TSearchRequest : EntitySearchRequest;
    }
}
