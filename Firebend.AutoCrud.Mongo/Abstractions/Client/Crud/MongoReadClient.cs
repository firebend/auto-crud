using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
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
        protected MongoReadClient(IMongoClient client,
            ILogger<MongoReadClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration) : base(client, logger, entityConfiguration)
        {
        }

        private async Task<IMongoQueryable<TEntity>> GetQueryableInternalAsync(FilterDefinition<TEntity> firstPipelineStateFilters,
            Expression<Func<TEntity, bool>> additionalFilter,
            CancellationToken cancellationToken)
        {
            var queryable = await GetFilteredCollectionAsync(firstPipelineStateFilters, cancellationToken).ConfigureAwait(false);

            if (additionalFilter != null)
            {
                queryable = queryable.Where(additionalFilter);
            }

            return queryable;
        }


        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            var query = await GetQueryableInternalAsync(null, filter, cancellationToken).ConfigureAwait(false);
            var entity = await RetryErrorAsync(() => query.FirstOrDefaultAsync(cancellationToken)).ConfigureAwait(false);
            return entity;
        }

        public Task<IMongoQueryable<TEntity>> GetQueryableAsync(FilterDefinition<TEntity> firstPipelineStateFilters,
            CancellationToken cancellationToken = default) => GetQueryableInternalAsync(firstPipelineStateFilters, null, cancellationToken);

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            var query = await GetQueryableInternalAsync(null, filter, cancellationToken).ConfigureAwait(false);
            var list = await RetryErrorAsync(() => query.ToListAsync(cancellationToken)).ConfigureAwait(false);

            return list;
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableInternalAsync(null, filter, cancellationToken).ConfigureAwait(false);
            var exists = await RetryErrorAsync(() => query.AnyAsync(cancellationToken)).ConfigureAwait(false);
            return exists;
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var query = await GetQueryableInternalAsync(null, filter, cancellationToken).ConfigureAwait(false);
            var exists = await RetryErrorAsync(() => query.LongCountAsync(cancellationToken)).ConfigureAwait(false);
            return exists;
        }

        public async Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IMongoQueryable<TEntity> queryable,
            TSearchRequest searchRequest,
            CancellationToken cancellationToken = default)
            where TSearchRequest : EntitySearchRequest
        {
            long? count = null;

            if (searchRequest?.DoCount ?? false)
            {
                count = await RetryErrorAsync(() => queryable.LongCountAsync(cancellationToken)).ConfigureAwait(false);
            }

            var orderBys = searchRequest?.OrderBy?.ToOrderByGroups<TEntity>()?.ToList();

            if (!orderBys.IsEmpty())
            {
                IOrderedMongoQueryable<TEntity> ordered = null;

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

            var data = await RetryErrorAsync(() => queryable.ToListAsync(cancellationToken))
                .ConfigureAwait(false);

            return new EntityPagedResponse<TEntity> { TotalRecords = count, Data = data, CurrentPage = searchRequest?.PageNumber, CurrentPageSize = searchRequest?.PageSize };
        }

    }
}
