using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Implementations;
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
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoRetryService mongoRetryService) : base(client, logger, mongoRetryService)
        {
            EntityConfiguration = entityConfiguration;
        }

        protected IMongoEntityConfiguration<TKey, TEntity> EntityConfiguration { get; }

        protected IMongoCollection<TEntity> GetCollection(IMongoEntityConfiguration<TKey, TEntity> configuration)
        {
            var database = Client.GetDatabase(configuration.DatabaseName);

            return database.GetCollection<TEntity>(configuration.CollectionName);
        }

        protected IMongoCollection<TEntity> GetCollection() => GetCollection(EntityConfiguration);

        protected async Task<IMongoQueryable<TEntity>> GetFilteredCollectionAsync(Func<IMongoQueryable<TEntity>, IMongoQueryable<TEntity>> firstStageFilters,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            var collection = GetCollection();

            var mongoQueryable = entityTransaction == null ?
                collection.AsQueryable() :
                collection.AsQueryable(UnwrapSession(entityTransaction));

            if (firstStageFilters != null)
            {
                mongoQueryable = firstStageFilters(mongoQueryable);
            }

            var filters = await BuildFiltersAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return filters == null ? mongoQueryable : mongoQueryable.Where(filters);
        }

        protected async Task<Expression<Func<TEntity, bool>>> BuildFiltersAsync(Expression<Func<TEntity, bool>> additionalFilter = null,
            CancellationToken cancellationToken = default)
        {
            var securityFilters = await GetSecurityFiltersAsync(cancellationToken).ConfigureAwait(false)
                                  ?? new List<Expression<Func<TEntity, bool>>>();

            var filters = securityFilters
                .Where(x => x != null)
                .ToList();

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

        protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IEnumerable<Expression<Func<TEntity, bool>>>>(null);

        protected virtual IClientSessionHandle UnwrapSession(IEntityTransaction entityTransaction) => entityTransaction switch
        {
            null => null,
            MongoEntityTransaction mongoTransaction => mongoTransaction.ClientSessionHandle,
            _ => throw new ArgumentException($"Is not a {nameof(MongoEntityTransaction)}", nameof(entityTransaction))
        };
    }
}
