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
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class EntityFrameworkQueryClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkQueryClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IEntityQueryOrderByHandler<TKey, TEntity> _orderByHandler;
    private readonly IEntityFrameworkIncludesProvider<TKey, TEntity> _includesProvider;

    public EntityFrameworkQueryClient(IDbContextProvider<TKey, TEntity> contextProvider,
        IEntityQueryOrderByHandler<TKey, TEntity> orderByHandler,
        IEntityFrameworkIncludesProvider<TKey, TEntity> includesProvider) : base(contextProvider)
    {
        _orderByHandler = orderByHandler;
        _includesProvider = includesProvider;
    }

    protected virtual async Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var context = await GetDbContextAsync(transaction, cancellationToken);

        var query = await GetFilteredQueryableAsync(context, asNoTracking, cancellationToken);

        query = await ModifyQueryableAsync(query);

        if (filter != null)
        {
            query = query.Where(filter);
        }

        return (query, context);
    }

    public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var (query, context) = await GetQueryableAsync(filter, asNoTracking, entityTransaction, cancellationToken);

        await using (context)
        {
            var entity = await query.FirstOrDefaultAsync(cancellationToken);
            return entity;
        }
    }

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken)
        => GetFirstOrDefaultAsync(filter, asNoTracking, null, cancellationToken);

    public Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        CancellationToken cancellationToken)
        => GetQueryableAsync(asNoTracking, null, cancellationToken);

    public Task<(IQueryable<TEntity> queryble, IDbContext context)> GetQueryableAsync(bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetQueryableAsync(null, asNoTracking, entityTransaction, cancellationToken);

    public Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
        => GetCountAsync(filter, null, cancellationToken);

    public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var (query, context) = await GetQueryableAsync(filter, true, entityTransaction, cancellationToken);

        await using (context)
        {
            var count = await query.LongCountAsync(cancellationToken);
            return count;
        }
    }

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        CancellationToken cancellationToken)
        => GetAllAsync(filter, asNoTracking, null, cancellationToken);

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        bool asNoTracking,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var (query, context) = await GetQueryableAsync(filter, asNoTracking, entityTransaction, cancellationToken);

        await using (context)
        {
            var list = await query.ToListAsync(cancellationToken);
            return list;
        }
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
        => ExistsAsync(filter, null, cancellationToken);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var (query, context) = await GetQueryableAsync(filter, true, entityTransaction, cancellationToken);

        await using (context)
        {
            var exists = await query.AnyAsync(cancellationToken);
            return exists;
        }
    }

    public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest,
        bool asNoTracking,
        CancellationToken cancellationToken)
        where TSearchRequest : IEntitySearchRequest
    {
        if (asNoTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        int? count = null;

        if (searchRequest?.DoCount ?? false)
        {
            count = await queryable.CountAsync(cancellationToken);
        }

        if (_orderByHandler != null && searchRequest is IOrderableSearchRequest orderableSearchRequest)
        {
            queryable = _orderByHandler.OrderBy(queryable, orderableSearchRequest.OrderBy?.ToOrderByGroups<TEntity>()?.ToList());
        }

        if (searchRequest is { PageNumber: > 0, PageSize: > 0 })
        {
            queryable = queryable
                .Skip((searchRequest.PageNumber.Value - 1) * searchRequest.PageSize.Value)
                .Take(searchRequest.PageSize.Value);
        }

        var list = await queryable.ToListAsync(cancellationToken);

        return new EntityPagedResponse<TEntity>
        {
            Data = list,
            CurrentPage = searchRequest?.PageNumber,
            TotalRecords = count,
            CurrentPageSize = list.Count
        };
    }

    protected virtual Task<IQueryable<TEntity>> ModifyQueryableAsync(IQueryable<TEntity> queryable)
        => Task.FromResult(queryable);

    protected override IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable) =>
        _includesProvider is null or DefaultEntityFrameworkIncludesProvider<TKey, TEntity> ? queryable : _includesProvider.AddIncludes(queryable);
}
