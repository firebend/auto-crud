using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkQueryClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkQueryClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkFullTextExpressionProvider<TKey, TEntity> _fullTextSearchProvider;

        public EntityFrameworkQueryClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityFrameworkFullTextExpressionProvider<TKey, TEntity> fullTextSearchProvider) : base(contextProvider)
        {
            _fullTextSearchProvider = fullTextSearchProvider;
        }

        public async Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            var entity = await base
                .GetByKeyAsync(context, key, cancellationToken)
                .ConfigureAwait(false);
            
            return entity;
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var queryable = await GetFilteredQueryableAsync(context, null, cancellationToken).ConfigureAwait(false);
            var list = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);
            
            return list;
        }

        public async Task<EntityPagedResponse<TEntity>> PageAsync(string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = true,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            int? count = null;

            if (doCount)
            {
                count = await CountAsync(search, filter, context, cancellationToken)
                    .ConfigureAwait(false);
            }

            var queryable = await BuildQueryAsync(context, search, filter, pageNumber, pageSize, orderBys, cancellationToken);

            var data = await queryable.ToListAsync(cancellationToken);

            return new EntityPagedResponse<TEntity>
            {
                TotalRecords = count,
                Data = data,
                CurrentPage = pageNumber,
                CurrentPageSize = pageSize
            };
        }

        public async Task<EntityPagedResponse<TOut>> PageAsync<TOut>(Expression<Func<TEntity, TOut>> projection,
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = false,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            int? count = null;

            if (doCount)
            {
                count = await CountAsync(search, filter, context, cancellationToken)
                    .ConfigureAwait(false);
            }

            var queryable = await BuildQueryAsync(context, search, filter, pageNumber, pageSize, orderBys, cancellationToken);

            var project = queryable.Select(projection);

            var data = await project.ToListAsync(cancellationToken);

            return new EntityPagedResponse<TOut>
            {
                TotalRecords = count,
                Data = data,
                CurrentPage = pageNumber,
                CurrentPageSize = pageSize
            };
        }

        public Task<int> CountAsync(
            string search,
            Expression<Func<TEntity, bool>> expression,
            CancellationToken cancellationToken = default)
            => CountAsync(search, expression, null, cancellationToken);

        private async Task<int> CountAsync(
            string search,
            Expression<Func<TEntity, bool>> expression,
            IDbContext dbContext = null,
            CancellationToken cancellationToken = default)
        {
            if (dbContext == null)
            {
                dbContext = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            }

            var queryable = await BuildQueryAsync(dbContext, search, expression, cancellationToken: cancellationToken);

            var count = await queryable
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);

            return count;
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var queryable = await BuildQueryAsync(context, filter: filter);

            var exists = await queryable
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);
            
            return exists;
        }
        
        protected async  Task<IQueryable<TEntity>> BuildQueryAsync(
            IDbContext context,
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default)
        {
            var queryable = await GetFilteredQueryableAsync(context);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchWhere = _fullTextSearchProvider?.Filter;
                
                if (searchWhere != null)
                {
                    var temp = searchWhere.FixParam(search);
                    queryable = queryable.Where(temp);
                }
            }

            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            if (orderBys != null)
            {
                IOrderedQueryable<TEntity> ordered = null;

                foreach (var orderBy in orderBys)
                {
                    if (orderBy != default)
                    {
                        ordered = ordered == null ? orderBy.ascending ? queryable.OrderBy(orderBy.order) :
                            queryable.OrderByDescending(orderBy.order) :
                            orderBy.ascending ? ordered.ThenBy(orderBy.order) :
                            ordered.ThenByDescending(orderBy.order);
                    }
                }

                if (ordered != null)
                {
                    queryable = ordered;
                }
            }

            if ((pageNumber ?? 0) > 0 && (pageSize ?? 0) > 0)
            {
                queryable = queryable
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            return queryable;
        }
    }
}