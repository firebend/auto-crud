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
        IMongoRetryService mongoRetryService,
        IMongoReadPreferenceService readPreferenceService) : base(factory, logger, mongoRetryService)
    {
        ReadPreferenceService = readPreferenceService;
        EntityConfiguration = entityConfiguration;
    }

    public IMongoReadPreferenceService ReadPreferenceService { get; }

    protected IMongoEntityConfiguration<TKey, TEntity> EntityConfiguration { get; }

    protected virtual async Task<IMongoCollection<TEntity>> GetCollectionAsync(
        IMongoEntityConfiguration<TKey, TEntity> configuration,
        string shardKeyOverride,
        bool isUsingTransaction,
        CancellationToken cancellationToken)
    {
        var client = await GetClientAsync(shardKeyOverride, cancellationToken);
        var database = client.GetDatabase(configuration.DatabaseName);

        var collection = database.GetCollection<TEntity>(configuration.CollectionName);

        var readPreference = ReadPreferenceService.GetMode();

        collection = isUsingTransaction ? collection
            : readPreference.HasValue
            ? collection.WithReadPreference(new ReadPreference(readPreference.Value))
            : collection;

        return collection;
    }

    protected virtual Task<IMongoCollection<TEntity>> GetCollectionAsync(string shardKeyOverride, CancellationToken cancellationToken)
        => GetCollectionAsync(EntityConfiguration, shardKeyOverride, false, cancellationToken);

    protected virtual Task<IQueryable<TEntity>> GetFilteredCollectionAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetFilteredCollectionAsync(x => Task.FromResult(firstStageFilters(x)),
            entityTransaction, cancellationToken);

    protected virtual async Task<IQueryable<TEntity>> GetFilteredCollectionAsync(
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync(null, cancellationToken);

        var mongoQueryable = entityTransaction == null ?
            collection.AsQueryable(EntityConfiguration.AggregateOption) :
            collection.AsQueryable(UnwrapSession(entityTransaction), EntityConfiguration.AggregateOption);

        if (firstStageFilters != null)
        {
            mongoQueryable = await firstStageFilters(mongoQueryable);
        }

        var filters = await BuildFiltersAsync(cancellationToken: cancellationToken);

        return filters == null ? mongoQueryable : mongoQueryable.Where(filters);
    }

    protected virtual async Task<Expression<Func<TEntity, bool>>> BuildFiltersAsync(Expression<Func<TEntity, bool>> additionalFilter = null,
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

    protected virtual Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Expression<Func<TEntity, bool>>>>(null);

    protected virtual IClientSessionHandle UnwrapSession(IEntityTransaction entityTransaction) => entityTransaction switch
    {
        null => null,
        MongoEntityTransaction mongoTransaction => mongoTransaction.ClientSessionHandle,
        _ => throw new ArgumentException($"Is not a {nameof(MongoEntityTransaction)}", nameof(entityTransaction))
    };
}
