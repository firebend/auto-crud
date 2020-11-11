using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions
{
    public abstract class AbstractDbContextRepo<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDbContextProvider<TKey, TEntity> _provider;

        private IDbContext _context;

        protected AbstractDbContextRepo(IDbContextProvider<TKey, TEntity> provider)
        {
            _provider = provider;
        }

        protected async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            if (_context == null)
            {
                _context = await _provider
                    .GetDbContextAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            return _context;
        }

        protected DbSet<TEntity> GetDbSet(IDbContext context)
        {
            return context.Set<TEntity>();
        }

        protected async Task<TEntity> GetByKeyAsync(IDbContext context, TKey key, CancellationToken cancellationToken)
        {
            return await ((await GetFilteredQueryableAsync(context, cancellationToken: cancellationToken)).FirstOrDefaultAsync(x => x.Id.Equals(key), cancellationToken));
        }

        protected async Task<IQueryable<TEntity>> GetFilteredQueryableAsync(
            IDbContext context,
            Expression<Func<TEntity, bool>> firstStageFilters = null,
            CancellationToken cancellationToken = default)
        {
            var set = GetDbSet(context);

            var queryable = set.AsQueryable();

            if (firstStageFilters != null)
                queryable = queryable.Where(firstStageFilters);

            var filters = await BuildFilters(cancellationToken: cancellationToken);

            return filters == null ? queryable : queryable.Where(filters);
        }

        protected async Task<Expression<Func<TEntity, bool>>> BuildFilters(Expression<Func<TEntity, bool>> additionalFilter = null, CancellationToken cancellationToken = default)
        {
            var securityFilters = await GetSecurityFiltersAsync(cancellationToken) ?? new List<Expression<Func<TEntity, bool>>>();

            var filters = securityFilters
                .Where(x => x != null)
                .ToList();

            if (additionalFilter != null)
                filters.Add(additionalFilter);

            if (filters.Count == 0)
                return null;

            return filters.Aggregate(default(Expression<Func<TEntity, bool>>),
                (aggregate, filter) => aggregate.AndAlso(filter));
        }

        protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IEnumerable<Expression<Func<TEntity, bool>>>)null);
        }
    }
}
