#region

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

#endregion

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

        protected IDbContext Context => _context ??= _provider.GetDbContext();

        protected DbSet<TEntity> GetDbSet()
        {
            return Context.Set<TEntity>();
        }

        protected Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken)
        {
            return GetFilteredQueryable().FirstOrDefaultAsync(x => x.Id.Equals(key), cancellationToken);
        }

        protected IQueryable<TEntity> GetFilteredQueryable(Expression<Func<TEntity, bool>> firstStageFilters = null)
        {
            var queryable = GetDbSet().AsQueryable();

            if (firstStageFilters != null) queryable = queryable.Where(firstStageFilters);

            var filters = BuildFilters();

            return filters == null ? queryable : queryable.Where(filters);
        }

        protected Expression<Func<TEntity, bool>> BuildFilters(Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var securityFilters = GetSecurityFilters() ?? new List<Expression<Func<TEntity, bool>>>();

            var filters = securityFilters
                .Where(x => x != null)
                .ToList();

            if (additionalFilter != null) filters.Add(additionalFilter);

            if (filters.Count == 0) return null;

            return filters.Aggregate(default(Expression<Func<TEntity, bool>>),
                (aggregate, filter) => aggregate.AndAlso(filter));
        }

        protected virtual IEnumerable<Expression<Func<TEntity, bool>>> GetSecurityFilters()
        {
            return null;
        }
    }
}