using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Client.Indexing;

public class MongoIndexClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoIndexClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IDistributedLockService _distributedLockService;
    private readonly IMongoIndexProvider<TKey, TEntity> _indexProvider;
    private readonly IMongoIndexMergeService<TKey, TEntity> _mongoIndexMergeService;

    public MongoIndexClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        ILogger<MongoIndexClient<TKey, TEntity>> logger,
        IMongoIndexProvider<TKey, TEntity> indexProvider,
        IMongoRetryService retryService,
        IDistributedLockService distributedLockService,
        IMongoIndexMergeService<TKey, TEntity> mongoIndexMergeService) : base(clientFactory, logger, entityConfiguration, retryService)
    {
        _indexProvider = indexProvider;
        _distributedLockService = distributedLockService;
        _mongoIndexMergeService = mongoIndexMergeService;
    }

    public async Task BuildIndexesAsync(IMongoEntityIndexConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken)
    {
        var key = $"{configuration.ShardKey}.{configuration.DatabaseName}.{configuration.CollectionName}.Indexes";
        using var _ = await _distributedLockService.LockAsync(key, cancellationToken);

        var builder = Builders<TEntity>.IndexKeys;
        var indexesToAdd = _indexProvider.GetIndexes(builder, configuration)?.ToArray();
        var hasIndexesToAdd = (indexesToAdd?.Length ?? 0) > 0;

        if (!hasIndexesToAdd)
        {
            return;
        }

        var dbCollection = await GetCollectionAsync(configuration, configuration.ShardKey, cancellationToken);

        await _mongoIndexMergeService.MergeIndexesAsync(dbCollection, indexesToAdd, cancellationToken);
    }

    public async Task CreateCollectionAsync(IMongoEntityIndexConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken)
    {
        var key = $"{configuration.ShardKey}.{configuration.DatabaseName}.{configuration.CollectionName}.CreateCollection";

        using var _ = await _distributedLockService.LockAsync(key, cancellationToken);

        var client = await GetClientAsync(configuration.ShardKey, cancellationToken);
        var database = client.GetDatabase(configuration.DatabaseName);

        var options = new ListCollectionNamesOptions { Filter = new BsonDocument("name", EntityConfiguration.CollectionName) };

        var collectionNames = await database.ListCollectionNamesAsync(options, cancellationToken);

        var collectionExists = await collectionNames.AnyAsync(cancellationToken);

        if (!collectionExists)
        {
            await database.CreateCollectionAsync(configuration.CollectionName, null, cancellationToken);
        }
    }

    protected override Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Enumerable.Empty<Expression<Func<TEntity, bool>>>());
}
