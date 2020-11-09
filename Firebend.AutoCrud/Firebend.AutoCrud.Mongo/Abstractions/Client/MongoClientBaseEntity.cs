using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public abstract class MongoClientBaseEntity<TKey, TEntity> : MongoClientBase
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        protected MongoClientBaseEntity(IMongoClient client,
            ILogger logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration) : base(client, logger)
        {
            EntityConfiguration = entityConfiguration;
        }

        protected IMongoEntityConfiguration<TKey, TEntity> EntityConfiguration { get; }

        protected IMongoCollection<TEntity> GetCollection()
        {
            var database = Client.GetDatabase(EntityConfiguration.DatabaseName);

            return database.GetCollection<TEntity>(EntityConfiguration.CollectionName);
        }

        protected async Task<IMongoQueryable<TEntity>> GetFilteredCollectionAsync(FilterDefinition<TEntity> firstStageFilters = null, CancellationToken cancellationToken = default)
        {
            var mongoQueryable = GetCollection().AsQueryable();

            if (firstStageFilters != null) mongoQueryable = mongoQueryable.Where(_ => firstStageFilters.Inject());

            var filters = await BuildFiltersAsync(cancellationToken: cancellationToken);

            return filters == null ? mongoQueryable : mongoQueryable.Where(filters);
        }

        protected async Task<Expression<Func<TEntity, bool>>> BuildFiltersAsync(Expression<Func<TEntity, bool>> additionalFilter = null, CancellationToken cancellationToken = default)
        {
            var securityFilters = await GetSecurityFiltersAsync(cancellationToken) ?? new List<Expression<Func<TEntity, bool>>>();

            var filters = securityFilters
                .Where(x => x != null)
                .ToList();

            if (additionalFilter != null) filters.Add(additionalFilter);

            if (filters.Count == 0) return null;

            return filters.Aggregate(default(Expression<Func<TEntity, bool>>),
                (aggregate, filter) => aggregate.AndAlso(filter));
        }

        protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Expression<Func<TEntity, bool>>>>(null);
        }
    }
}