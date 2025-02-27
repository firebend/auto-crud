using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoReadClient<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest,
        CancellationToken cancellationToken)
        where TSearchRequest : IEntitySearchRequest;
}
