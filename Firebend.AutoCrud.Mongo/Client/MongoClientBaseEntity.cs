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

namespace Firebend.AutoCrud.Mongo.Client;

public abstract class MongoClientBaseEntity<TKey, TEntity> : MongoClientBase<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    protected MongoClientBaseEntity(IMongoClientFactory<TKey, TEntity> factory,
        ILogger logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService) : base(factory, logger, mongoRetryService)
    {
        EntityConfiguration = entityConfiguration;
    }

    protected IMongoEntityConfiguration<TKey, TEntity> EntityConfiguration { get; }

    protected virtual Task<IMongoCollection<TEntity>> GetCollectionAsync(
        CancellationToken cancellationToken)
        => GetCollectionAsync(null, null, false, cancellationToken);

    protected virtual async Task<IMongoCollection<TEntity>> GetCollectionAsync(
        IMongoEntityConfiguration<TKey, TEntity> configurationOverride,
        string shardKeyOverride,
        bool isUsingTransaction,
        CancellationToken cancellationToken)
    {
        var configuration = configurationOverride ?? EntityConfiguration;
        var client = await GetClientAsync(shardKeyOverride, cancellationToken);
        var database = client.GetDatabase(configuration.DatabaseName);

        var collection = database.GetCollection<TEntity>(configuration.CollectionName);

        if (isUsingTransaction is false && EntityConfiguration.ReadPreferenceMode.HasValue)
        {
            collection = EntityConfiguration.ReadPreferenceMode.Value switch
            {
                ReadPreferenceMode.Primary => collection.WithReadPreference(ReadPreference.Primary),
                ReadPreferenceMode.PrimaryPreferred => collection.WithReadPreference(ReadPreference.PrimaryPreferred),
                ReadPreferenceMode.Secondary => collection.WithReadPreference(ReadPreference.Secondary),
                ReadPreferenceMode.SecondaryPreferred => collection.WithReadPreference(ReadPreference.SecondaryPreferred),
                ReadPreferenceMode.Nearest => collection.WithReadPreference(ReadPreference.Nearest),
                _ => collection
            };
        }

        return collection;
    }

    protected virtual async Task<IMongoQueryable<TEntity>> GetFilteredCollectionAsync(
        Func<IMongoQueryable<TEntity>, Task<IMongoQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var isUsingTransaction = entityTransaction is not null;
        var collection = await GetCollectionAsync(null, null, isUsingTransaction, cancellationToken);

        var mongoQueryable = isUsingTransaction is false
            ? collection.AsQueryable(EntityConfiguration.AggregateOption)
            : collection.AsQueryable(UnwrapSession(entityTransaction), EntityConfiguration.AggregateOption);

        if (firstStageFilters is not null)
        {
            mongoQueryable = await firstStageFilters(mongoQueryable);
        }

        var filters = await BuildFiltersAsync(cancellationToken: cancellationToken);

        return filters == null ? mongoQueryable : mongoQueryable.Where(filters);
    }

    protected virtual async Task<Expression<Func<TEntity, bool>>> BuildFiltersAsync(
        Expression<Func<TEntity, bool>> additionalFilter = null,
        CancellationToken cancellationToken = default)
    {
        var filters = new List<Expression<Func<TEntity, bool>>>();

        var securityFilters = await GetSecurityFiltersAsync(cancellationToken);

        if (securityFilters is not null)
        {
            filters.AddRange(securityFilters);
        }

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

    protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(
        CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Expression<Func<TEntity, bool>>>>(null);

    protected virtual IClientSessionHandle UnwrapSession(IEntityTransaction entityTransaction) =>
        entityTransaction switch
        {
            null => null,
            MongoEntityTransaction mongoTransaction => mongoTransaction.ClientSessionHandle,
            _ => throw new ArgumentException($"Is not a {nameof(MongoEntityTransaction)}", nameof(entityTransaction))
        };
}
