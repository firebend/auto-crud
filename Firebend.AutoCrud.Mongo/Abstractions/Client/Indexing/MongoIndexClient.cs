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
        private readonly IMemoizer<bool> _memoizer;
        private readonly IMongoIndexMergeService _mongoIndexMergeService;

        public MongoIndexClient(IMongoClient client,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ILogger<MongoIndexClient<TKey, TEntity>> logger,
            IMongoIndexProvider<TEntity> indexProvider,
            IMongoRetryService retryService,
            IDistributedLockService distributedLockService,
            IMongoIndexMergeService mongoIndexMergeService,
            IMemoizer<bool> memoizer) : base(client, logger, entityConfiguration, retryService)
        {
            _indexProvider = indexProvider;
            _distributedLockService = distributedLockService;
            _mongoIndexMergeService = mongoIndexMergeService;
            _memoizer = memoizer;
        }

        public Task BuildIndexesAsync(IMongoEntityConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken = default)
            => CheckConfiguredAsync($"{configuration.DatabaseName}.{configuration.CollectionName}.Indexes",
                async () =>
                {
                    var builder = Builders<TEntity>.IndexKeys;
                    var indexesToAdd = _indexProvider.GetIndexes(builder)?.ToArray();
                    var hasIndexesToAdd = indexesToAdd?.Any() ?? false;

                    if (!hasIndexesToAdd)
                    {
                        return;
                    }

                    var dbCollection = GetCollection(configuration);

                    await _mongoIndexMergeService.MergeIndexesAsync(dbCollection, indexesToAdd, cancellationToken);
                }, cancellationToken);

        public Task CreateCollectionAsync(IMongoEntityConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken = default)
            => CheckConfiguredAsync($"{configuration.DatabaseName}.{configuration.CollectionName}.CreateCollection", () => RetryErrorAsync(async () =>
            {
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
            }), cancellationToken);

        private Task CheckConfiguredAsync(string configurationKey,
            Func<Task> configure,
            CancellationToken cancellationToken) => _memoizer.MemoizeAsync<(
            MongoIndexClient<TKey, TEntity> self,
            string key,
            Func<Task> configure,
            CancellationToken cancellationToken)>(configurationKey, static async arg =>
        {
            var locker = await arg.self._distributedLockService
                .LockAsync(arg.key, arg.cancellationToken)
                .ConfigureAwait(false);

            using (locker)
            {
                await arg.configure().ConfigureAwait(false);
            }

            return true;
        }, (this, configurationKey, configure, cancellationToken), cancellationToken);

        protected override Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Expression<Func<TEntity, bool>>>());
    }
}
