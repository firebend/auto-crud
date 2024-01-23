using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkQueryClient<TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken = default);

    Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default);

    Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        CancellationToken cancellationToken = default);

    Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default);

    Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest,
        bool asNoTracking,
        CancellationToken cancellationToken = default)
        where TSearchRequest : IEntitySearchRequest;
}
