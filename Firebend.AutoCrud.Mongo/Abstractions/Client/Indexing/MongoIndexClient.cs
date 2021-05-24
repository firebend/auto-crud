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
        private readonly IMongoIndexProvider<TEntity> _indexProvider;
        private readonly IDistributedLockService _distributedLockService;

        public MongoIndexClient(IMongoClient client,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ILogger<MongoIndexClient<TKey, TEntity>> logger,
            IMongoIndexProvider<TEntity> indexProvider,
            IMongoRetryService retryService,
            IDistributedLockService distributedLockService) : base(client, logger, entityConfiguration, retryService)
        {
            _indexProvider = indexProvider;
            _distributedLockService = distributedLockService;
        }

        public Task BuildIndexesAsync(IMongoEntityConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken = default)
            => CheckConfiguredAsync($"{configuration.DatabaseName}.{configuration.CollectionName}.Indexes",
                async () =>
                {
                    var dbCollection = GetCollection(configuration);
                    var builder = Builders<TEntity>.IndexKeys;
                    var indexesToAdd = _indexProvider.GetIndexes(builder)?.ToArray();

                    if (!(indexesToAdd?.Any() ?? false))
                    {
                        return;
                    }

                    var indexesCursor = await dbCollection
                        .Indexes
                        .ListAsync(cancellationToken)
                        .ConfigureAwait(false);

                    var indexes = await indexesCursor
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);

                    var existingTextIndex = indexes.FirstOrDefault(y => y.Contains("textIndexVersion"));

                    if (existingTextIndex != null && indexesToAdd.Any(y => y?.Options?.Name != null && y.Options.Name.Equals("text")))
                    {
                        Logger.LogDebug("Dropping text index {IndexName} for collection {CollectionName}",
                            existingTextIndex["name"].AsString,
                            configuration.CollectionName);

                        await RetryErrorAsync(() => dbCollection
                                .Indexes
                                .DropOneAsync(existingTextIndex["name"].AsString, cancellationToken))
                            .ConfigureAwait(false);
                    }

                    await RetryErrorAsync(() => dbCollection
                            .Indexes
                            .CreateManyAsync(indexesToAdd, cancellationToken))
                        .ConfigureAwait(false);
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
                        .CreateCollectionAsync($"{configuration.DatabaseName}.{configuration.CollectionName}", null, cancellationToken)
                        .ConfigureAwait(false);
                }
            }), cancellationToken);

        private async Task CheckConfiguredAsync(string configurationKey,
            Func<Task> configure,
            CancellationToken cancellationToken)
        {
            if (MongoIndexClientConfigurations.Configurations.TryGetValue(configurationKey, out var configured))
            {
                if (configured)
                {
                    return;
                }
            }

            using (await _distributedLockService.LockAsync(configurationKey, cancellationToken)
                .ConfigureAwait(false))
            {
                if (MongoIndexClientConfigurations.Configurations.TryGetValue(configurationKey, out configured))
                {
                    if (configured)
                    {
                        return;
                    }
                }

                await configure();

                MongoIndexClientConfigurations.Configurations[configurationKey] = true;
            }
        }

        protected override Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Enumerable.Empty<Expression<Func<TEntity, bool>>>());
    }
}
