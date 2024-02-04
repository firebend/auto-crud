using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Implementations;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Abstractions;

public abstract class AbstractDbContextRepo<TKey, TEntity> : BaseDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IDbContextProvider<TKey, TEntity> _provider;

    protected AbstractDbContextRepo(IDbContextProvider<TKey, TEntity> provider)
    {
        _provider = provider;
    }

    protected virtual async Task<IDbContext> GetDbContextAsync(IEntityTransaction entityTransaction, CancellationToken cancellationToken)
    {
        IDbContext context;

        if (entityTransaction == null)
        {
            context = await _provider.GetDbContextAsync(cancellationToken);
        }
        else
        {
            if (entityTransaction is not EntityFrameworkEntityTransaction efTransaction)
            {
                throw new ArgumentException($"Transaction is not a {nameof(EntityFrameworkEntityTransaction)}", nameof(entityTransaction));
            }

            var transaction = efTransaction.ContextTransaction.GetDbTransaction();

            context = await _provider.GetDbContextAsync(transaction, cancellationToken);
        }

        return context;
    }

    protected virtual DbSet<TEntity> GetDbSet(IDbContext context) => context.Set<TEntity>();

    protected virtual async Task<TEntity> GetByEntityKeyAsync(IDbContext context, TKey key, bool asNoTracking, CancellationToken cancellationToken)
    {
        var queryable = await GetFilteredQueryableAsync(context, asNoTracking, cancellationToken);

        var first = await queryable.FirstOrDefaultAsync(x => x.Id.Equals(key), cancellationToken);

        return first;
    }

    protected virtual async Task<IQueryable<TEntity>> GetFilteredQueryableAsync(
        IDbContext context,
        bool asNoTracking,
        CancellationToken cancellationToken = default)
    {
        var set = GetDbSet(context);

        var queryable = set.AsQueryable();

        if (asNoTracking)
        {
            queryable = queryable.AsNoTracking();
        }

        queryable = AddIncludes(queryable);

        var filters = await BuildFilters(cancellationToken: cancellationToken);

        return filters == null ? queryable : queryable.Where(filters);
    }

    protected virtual async Task<Expression<Func<TEntity, bool>>> BuildFilters(Expression<Func<TEntity, bool>> additionalFilter = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<Expression<Func<TEntity, bool>>>();

        var securityFilters = await GetSecurityFiltersAsync(cancellationToken);

        if (securityFilters is not null)
        {
            filters.AddRange(securityFilters);
        }

        if (additionalFilter != null)
        {
            filters.Add(additionalFilter);
        }

        if (filters.Count == 0)
        {
            return null;
        }

        return filters.Aggregate(default(Expression<Func<TEntity, bool>>),
            (aggregate, filter) => aggregate.AndAlso(filter));
    }

    protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IEnumerable<Expression<Func<TEntity, bool>>>)null);

    protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable) => queryable;
}
