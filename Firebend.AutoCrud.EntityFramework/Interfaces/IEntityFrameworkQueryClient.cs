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
    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken);

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        CancellationToken cancellationToken);

    public Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest,
        bool asNoTracking,
        CancellationToken cancellationToken)
        where TSearchRequest : IEntitySearchRequest;
}
