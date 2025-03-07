using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Client.Crud;

public class MongoReadClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoReadClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IEntityQueryOrderByHandler<TKey, TEntity> _orderByHandler;
    public MongoReadClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoReadClient<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IEntityQueryOrderByHandler<TKey, TEntity> orderByHandler,
        IMongoRetryService mongoRetryService,
        IMongoReadPreferenceService readPreferenceService) : base(
            clientFactory,
            logger,
            entityConfiguration,
            mongoRetryService,
            readPreferenceService)
    {
        _orderByHandler = orderByHandler;
    }

    protected virtual Task<IQueryable<TEntity>> GetQueryableInternalAsync(IEntityTransaction entityTransaction,
        Expression<Func<TEntity, bool>> additionalFilter,
        CancellationToken cancellationToken)
        => GetQueryableInternalAsync((Func<IQueryable<TEntity>, IQueryable<TEntity>>)null, entityTransaction, additionalFilter, cancellationToken);

    protected virtual Task<IQueryable<TEntity>> GetQueryableInternalAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        IEntityTransaction entityTransaction,
        Expression<Func<TEntity, bool>> additionalFilter,
        CancellationToken cancellationToken)
        => GetQueryableInternalAsync(firstStageFilters is not null ?
            x => Task.FromResult(firstStageFilters(x))
            : Task.FromResult, entityTransaction, additionalFilter, cancellationToken);

    protected virtual async Task<IQueryable<TEntity>> GetQueryableInternalAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction,
        Expression<Func<TEntity, bool>> additionalFilter,
        CancellationToken cancellationToken)
    {
        var queryable = await GetFilteredCollectionAsync(firstStageFilters, entityTransaction, cancellationToken);

        if (additionalFilter != null)
        {
            queryable = queryable.Where(additionalFilter);
        }

        return queryable;
    }


    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => GetFirstOrDefaultAsync(filter, null, cancellationToken);

    public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var query = await GetQueryableInternalAsync(entityTransaction, filter, cancellationToken);

        var entity = await RetryErrorAsync(() => query.FirstOrDefaultAsync(cancellationToken));
        return entity;
    }

    public Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken)
        => GetQueryableAsync((Func<IQueryable<TEntity>, IQueryable<TEntity>>)null, null, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        CancellationToken cancellationToken)
        => GetQueryableAsync(firstStageFilters, null, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        CancellationToken cancellationToken)
        => GetQueryableAsync(firstStageFilters, null, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetQueryableAsync((Func<IQueryable<TEntity>, IQueryable<TEntity>>)null, entityTransaction, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetQueryableInternalAsync(firstStageFilters, entityTransaction, null, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetQueryableInternalAsync(firstStageFilters, entityTransaction, null, cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => GetAllAsync(filter, null, cancellationToken);

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var query = await GetQueryableInternalAsync(entityTransaction, filter, cancellationToken);

        var list = await RetryErrorAsync(() => query.ToListAsync(cancellationToken));

        return list;
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => ExistsAsync(filter, null, cancellationToken);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var query = await GetQueryableInternalAsync(entityTransaction, filter, cancellationToken);

        var exists = await RetryErrorAsync(() => query.AnyAsync(cancellationToken));
        return exists;
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => CountAsync(filter, null, cancellationToken);

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction, CancellationToken cancellationToken)
    {
        var query = await GetQueryableInternalAsync(entityTransaction, filter, cancellationToken);

        var count = await RetryErrorAsync(() => query.LongCountAsync(cancellationToken));
        return count;
    }

    public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest,
        CancellationToken cancellationToken)
        where TSearchRequest : IEntitySearchRequest
    {
        long? count = null;

        if (searchRequest?.DoCount ?? false)
        {
            var queryable1 = queryable;
            count = await RetryErrorAsync(() => queryable1.LongCountAsync(cancellationToken));
        }

        if (searchRequest is IOrderableSearchRequest orderableSearchRequest)
        {
            queryable = _orderByHandler.OrderBy(queryable, orderableSearchRequest.OrderBy?.ToOrderByGroups<TEntity>()?.ToList());
        }

        if (searchRequest?.PageNumber != null
            && searchRequest.PageSize != null
            && searchRequest.PageNumber > 0
            && searchRequest.PageSize > 0)
        {
            queryable = queryable.Skip((searchRequest.PageNumber.Value - 1) * searchRequest.PageSize.Value)
                .Take(searchRequest.PageSize.Value);
        }

        var data = await RetryErrorAsync(() => queryable.ToListAsync(cancellationToken));

        return new EntityPagedResponse<TEntity>
        {
            TotalRecords = count,
            Data = data,
            CurrentPage = searchRequest?.PageNumber,
            CurrentPageSize = data.Count
        };
    }

}
