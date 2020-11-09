using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
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
        public MongoReadClient(IMongoClient client,
            ILogger<MongoReadClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration) : base(client, logger, entityConfiguration)
        {
        }
        
        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var queryable = await BuildQueryAsync(filter: filter, cancellationToken: cancellationToken);

            return await RetryErrorAsync(() => queryable.SingleOrDefaultAsync(cancellationToken));
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var queryable = await BuildQueryAsync(cancellationToken: cancellationToken);

            return await RetryErrorAsync(() => queryable.ToListAsync(cancellationToken));
        }

        public async Task<EntityPagedResponse<TEntity>> PageAsync(
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = true,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default)
        {
            int? count = null;

            if (doCount)
                count = await CountAsync(search, filter, cancellationToken)
                    .ConfigureAwait(false);

            var queryable = await BuildQueryAsync(search, filter, pageNumber, pageSize, orderBys, cancellationToken);

            var data = await RetryErrorAsync(() => queryable.ToListAsync(cancellationToken))
                .ConfigureAwait(false);

            return new EntityPagedResponse<TEntity>
            {
                TotalRecords = count,
                Data = data,
                CurrentPage = pageNumber,
                CurrentPageSize = pageSize
            };
        }

        public async Task<EntityPagedResponse<TOut>> PageAsync<TOut>(
            Expression<Func<TEntity, TOut>> projection,
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            bool doCount = false,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null,
            CancellationToken cancellationToken = default)
        {
            int? count = null;

            if (doCount)
            {
                count = await CountAsync(search, filter, cancellationToken)
                    .ConfigureAwait(false);
            }

            var queryable = await BuildQueryAsync(search, filter, pageNumber, pageSize, orderBys, cancellationToken);

            var project = queryable.Select(projection);

            var data = await RetryErrorAsync(() => project.ToListAsync(cancellationToken))
                .ConfigureAwait(false);

            return new EntityPagedResponse<TOut>
            {
                TotalRecords = count,
                Data = data,
                CurrentPage = pageNumber,
                CurrentPageSize = pageSize
            };
        }

        public async Task<int> CountAsync(string search, Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
        {
            var queryable = await BuildQueryAsync(search, expression, cancellationToken: cancellationToken);

            return await RetryErrorAsync(() => queryable.CountAsync(cancellationToken));
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            var queryable = await BuildQueryAsync(filter: filter, cancellationToken: cancellationToken);

            return await RetryErrorAsync(() => queryable.AnyAsync(cancellationToken));
        }

        protected async Task<IMongoQueryable<TEntity>> BuildQueryAsync(
            string search = null,
            Expression<Func<TEntity, bool>> filter = null,
            int? pageNumber = null,
            int? pageSize = null,
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys = null, CancellationToken cancellationToken = default)
        {
            FilterDefinition<TEntity> firstStageFilter = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                firstStageFilter = Builders<TEntity>.Filter.Text(search);
            }

            var queryable = await GetFilteredCollectionAsync(firstStageFilter, cancellationToken);

            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            if (orderBys != null)
            {
                IOrderedMongoQueryable<TEntity> ordered = null;

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