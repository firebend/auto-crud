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

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoReadClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoReadClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityQueryOrderByHandler<TKey, TEntity> _orderByHandler;
        protected MongoReadClient(IMongoClient client,
            ILogger<MongoReadClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityQueryOrderByHandler<TKey, TEntity> orderByHandler,
            IMongoRetryService mongoRetryService) : base(client, logger, entityConfiguration, mongoRetryService)
        {
            _orderByHandler = orderByHandler;
        }

        protected virtual async Task<IMongoQueryable<TEntity>> GetQueryableInternalAsync(Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilters,
            IEntityTransaction entityTransaction,
            Expression<Func<TEntity, bool>> additionalFilter,
            CancellationToken cancellationToken)
        {
            var queryable = await GetFilteredCollectionAsync(firstStageFilters, entityTransaction, cancellationToken).ConfigureAwait(false);

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
            var query = await GetQueryableInternalAsync(null, entityTransaction, filter, cancellationToken)
                .ConfigureAwait(false);

            var entity = await RetryErrorAsync(() => query.FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
            return entity;
        }

        public Task<IMongoQueryable<TEntity>> GetQueryableAsync(Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilters,
            CancellationToken cancellationToken = default)
            => GetQueryableAsync(firstStageFilters, null, cancellationToken);

        public Task<IMongoQueryable<TEntity>> GetQueryableAsync(Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilters,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => GetQueryableInternalAsync(firstStageFilters, entityTransaction, null, cancellationToken);

        public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
            => GetAllAsync(filter, null, cancellationToken);

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken)
        {
            var query = await GetQueryableInternalAsync(null, entityTransaction, filter, cancellationToken)
                .ConfigureAwait(false);

            var list = await RetryErrorAsync(() => query.ToListAsync(cancellationToken)).ConfigureAwait(false);

            return list;
        }

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            => ExistsAsync(filter, null, cancellationToken);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableInternalAsync(null, entityTransaction, filter, cancellationToken)
                .ConfigureAwait(false);

            var exists = await RetryErrorAsync(() => query.AnyAsync(cancellationToken)).ConfigureAwait(false);
            return exists;
        }

        public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            => CountAsync(filter, null, cancellationToken);

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableInternalAsync(null, entityTransaction, filter, cancellationToken)
                .ConfigureAwait(false);

            var count = await RetryErrorAsync(() => query.LongCountAsync(cancellationToken)).ConfigureAwait(false);
            return count;
        }

        public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IMongoQueryable<TEntity> queryable,
            TSearchRequest searchRequest,
            CancellationToken cancellationToken = default)
            where TSearchRequest : IEntitySearchRequest
        {
            long? count = null;

            if (searchRequest?.DoCount ?? false)
            {
                var queryable1 = queryable;
                count = await RetryErrorAsync(() => queryable1.LongCountAsync(cancellationToken)).ConfigureAwait(false);
            }

            if (searchRequest is IOrderableSearchRequest orderableSearchRequest)
            {
                queryable = _orderByHandler.OrderBy(queryable, orderableSearchRequest?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList());
            }

            if (searchRequest?.PageNumber != null
                && searchRequest.PageSize != null
                && searchRequest.PageNumber > 0
                && searchRequest.PageSize > 0)
            {
                queryable = queryable.Skip((searchRequest.PageNumber.Value - 1) * searchRequest.PageSize.Value)
                    .Take(searchRequest.PageSize.Value);
            }

            var data = await RetryErrorAsync(() => queryable.ToListAsync(cancellationToken))
                .ConfigureAwait(false);

            return new EntityPagedResponse<TEntity>
            {
                TotalRecords = count,
                Data = data,
                CurrentPage = searchRequest?.PageNumber,
                CurrentPageSize = data.Count
            };
        }

    }
}
