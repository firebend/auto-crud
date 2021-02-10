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
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkQueryClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkQueryClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityQueryOrderByHandler<TKey, TEntity> _orderByHandler;
        private readonly IEntityFrameworkIncludesProvider<TKey, TEntity> _includesProvider;

        protected EntityFrameworkQueryClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityQueryOrderByHandler<TKey, TEntity> orderByHandler,
            IEntityFrameworkIncludesProvider<TKey, TEntity> includesProvider) : base(contextProvider)
        {
            _orderByHandler = orderByHandler;
            _includesProvider = includesProvider;
        }

        private async Task<IQueryable<TEntity>> GetQueryableAsync(Expression<Func<TEntity, bool>> filter,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);

            var query = await GetFilteredQueryableAsync(context, asNoTracking, cancellationToken);

            query = await ModifyQueryableAsync(query);

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, asNoTracking, cancellationToken);
            var entity = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return entity;
        }

        public Task<IQueryable<TEntity>> GetQueryableAsync(bool asNoTracking, CancellationToken cancellationToken = default)
            => GetQueryableAsync(null, asNoTracking, cancellationToken);

        public async Task<long> GetCountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, true, cancellationToken).ConfigureAwait(false);
            var count = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, asNoTracking, cancellationToken).ConfigureAwait(false);
            var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return list;
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableAsync(filter, true, cancellationToken).ConfigureAwait(false);
            var exists = await query.AnyAsync(cancellationToken).ConfigureAwait(false);

            return exists;
        }

        public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
            TSearchRequest searchRequest,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TSearchRequest : EntitySearchRequest
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

            if (_orderByHandler != null)
            {
                queryable = _orderByHandler.OrderBy(queryable, searchRequest?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList());
            }

            if(searchRequest?.PageNumber != null && searchRequest.PageSize != null && searchRequest.PageNumber > 0 && searchRequest.PageSize > 0)
            {
                queryable = queryable
                    .Skip((searchRequest.PageNumber.Value - 1) * searchRequest.PageSize.Value)
                    .Take(searchRequest.PageSize.Value);
            }

            var list = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new EntityPagedResponse<TEntity>
            {
                Data = list,
                CurrentPage = searchRequest?.PageNumber,
                TotalRecords = count,
                CurrentPageSize = searchRequest?.PageSize
            };
        }

        protected virtual Task<IQueryable<TEntity>> ModifyQueryableAsync(IQueryable<TEntity> queryable)
            => Task.FromResult(queryable);

        protected override IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable)
            => _includesProvider != null ? _includesProvider.AddIncludes(queryable) : queryable;
    }
}
