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

        protected EntityFrameworkQueryClient(IDbContextProvider<TKey, TEntity> contextProvider) : base(contextProvider)
        {
        }

        private async Task<IQueryable<TEntity>> GetQueryableAsync(Expression<Func<TEntity, bool>> filter, bool track, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var query = await GetFilteredQueryableAsync(context, cancellationToken);
            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = await ModifyQueryableAsync(query);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, bool track, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, track, cancellationToken);
            var entity =  await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }

        public Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken = default)
            => GetQueryableAsync(null, true, cancellationToken);

        public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, false, cancellationToken).ConfigureAwait(false);
            var count = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, true, cancellationToken).ConfigureAwait(false);
            var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, false, cancellationToken).ConfigureAwait(false);
            var exists = await query.AnyAsync(cancellationToken).ConfigureAwait(false);

            return exists;
        }

        public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable, TSearchRequest searchRequest, CancellationToken cancellationToken = default)
            where TSearchRequest : EntitySearchRequest
        {
            queryable = queryable.AsNoTracking();

            int? count = null;

            if (searchRequest?.DoCount ?? false)
            {
                count = await queryable.CountAsync(cancellationToken);
            }

            var orderBys = searchRequest?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList();

            if (!orderBys.IsEmpty())
            {
                IOrderedQueryable<TEntity> ordered = null;

                foreach (var (orderExpression, @ascending) in orderBys.Where(orderBy => orderBy != default))
                {
                    if (ordered == null)
                    {
                        ordered = @ascending ? queryable.OrderBy(orderExpression) : queryable.OrderByDescending(orderExpression);
                    }
                    else
                    {
                        ordered = @ascending ? ordered.ThenBy(orderExpression) : ordered.ThenByDescending(orderExpression);
                    }
                }

                if (ordered != null)
                {
                    queryable = ordered;
                }
            }

            if ((searchRequest?.PageNumber ?? 0) > 0 && (searchRequest.PageSize ?? 0) > 0)
            {
                queryable = queryable
                    .Skip((searchRequest.PageNumber.Value - 1) * searchRequest.PageSize.Value)
                    .Take(searchRequest.PageSize.Value);
            }

            var list = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new EntityPagedResponse<TEntity>
            {
                Data = list, CurrentPage = searchRequest?.PageNumber, TotalRecords = count, CurrentPageSize = searchRequest?.PageSize
            };
        }

        protected virtual Task<IQueryable<TEntity>> ModifyQueryableAsync(IQueryable<TEntity> queryable) => Task.FromResult(queryable);
    }
}
