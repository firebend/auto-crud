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

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing
{
    public abstract class MongoIndexClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoIndexClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDistributedLockService _distributedLockService;
        private readonly IMongoIndexProvider<TEntity> _indexProvider;
        private readonly IMongoIndexMergeService _mongoIndexMergeService;

        public MongoIndexClient(IMongoClient client,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ILogger<MongoIndexClient<TKey, TEntity>> logger,
            IMongoIndexProvider<TEntity> indexProvider,
            IMongoRetryService retryService,
            IDistributedLockService distributedLockService,
            IMongoIndexMergeService mongoIndexMergeService) : base(client, logger, entityConfiguration, retryService)
        {
            _indexProvider = indexProvider;
            _distributedLockService = distributedLockService;
            _mongoIndexMergeService = mongoIndexMergeService;
        }

        public async Task BuildIndexesAsync(IMongoEntityConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken = default)
        {
            var key = $"{configuration.DatabaseName}.{configuration.CollectionName}.Indexes";

            using var _ = await _distributedLockService
                .LockAsync(key, cancellationToken)
                .ConfigureAwait(false);

            var builder = Builders<TEntity>.IndexKeys;
            var indexesToAdd = _indexProvider.GetIndexes(builder)?.ToArray();
            var hasIndexesToAdd = indexesToAdd?.Any() ?? false;

            if (!hasIndexesToAdd)
            {
                return;
            }

            var dbCollection = GetCollection(configuration);

            await _mongoIndexMergeService.MergeIndexesAsync(dbCollection, indexesToAdd, cancellationToken);
        }

        public async Task CreateCollectionAsync(IMongoEntityConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken = default)
        {
            var key = $"{configuration.DatabaseName}.{configuration.CollectionName}.CreateCollection";

            using var _ = await _distributedLockService
                .LockAsync(key, cancellationToken)
                .ConfigureAwait(false);

            var database = Client.GetDatabase(configuration.DatabaseName);

            var options = new ListCollectionNamesOptions { Filter = new BsonDocument("name", EntityConfiguration.CollectionName) };

            var collectionNames = await database
                .ListCollectionNamesAsync(options, cancellationToken)
                .ConfigureAwait(false);

            var collectionExists = await collectionNames
                .AnyAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!collectionExists)
            {
                await database
                    .CreateCollectionAsync(configuration.CollectionName, null, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        protected override Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Expression<Func<TEntity, bool>>>());
    }
}
