using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Threading;
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

        public MongoIndexClient(IMongoClient client,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ILogger<MongoIndexClient<TKey, TEntity>> logger,
            IMongoIndexProvider<TEntity> indexProvider) : base(client, logger, entityConfiguration)
        {
            _indexProvider = indexProvider;
        }

        public Task BuildIndexesAsync(CancellationToken cancellationToken = default)
        {
            return CheckConfiguredAsync($"{EntityConfiguration.CollectionName}.Indexes", async () =>
            {
                var dbCollection = GetCollection();
                var builder = Builders<TEntity>.IndexKeys;
                var indexesToAdd = _indexProvider.GetIndexes(builder)?.ToArray();

                if (!(indexesToAdd?.Any() ?? false)) return;

                var indexesCursor = await dbCollection.Indexes.ListAsync(cancellationToken).ConfigureAwait(false);

                var indexes = await indexesCursor.ToListAsync(cancellationToken).ConfigureAwait(false);

                var existingTextIndex = indexes.FirstOrDefault(y => y.Contains("textIndexVersion"));

                if (existingTextIndex != null && indexesToAdd.Any(y => y?.Options?.Name != null && y.Options.Name.Equals("text")))
                {
                    Logger.LogDebug($"Dropping text index {existingTextIndex["name"].AsString} for collection {EntityConfiguration.CollectionName}");

                    await RetryErrorAsync(
                            () => dbCollection.Indexes.DropOneAsync(existingTextIndex["name"].AsString, cancellationToken))
                        .ConfigureAwait(false);
                }

                await RetryErrorAsync(() => dbCollection.Indexes.CreateManyAsync(indexesToAdd, cancellationToken))
                    .ConfigureAwait(false);
            }, cancellationToken);
        }

        public Task CreateCollectionAsync(CancellationToken cancellationToken = default)
        {
            return CheckConfiguredAsync($"{EntityConfiguration.CollectionName}.CreateCollection", () => RetryErrorAsync(async () =>
            {
                var database = Client.GetDatabase(EntityConfiguration.DatabaseName);

                var collectionExists = await (await database
                    .ListCollectionNamesAsync(new ListCollectionNamesOptions
                    {
                        Filter = new BsonDocument("name", EntityConfiguration.CollectionName)
                    }, cancellationToken)).AnyAsync(cancellationToken).ConfigureAwait(false);

                if (!collectionExists) await database.CreateCollectionAsync(EntityConfiguration.CollectionName, null, cancellationToken);
            }), cancellationToken);
        }

        private async Task CheckConfiguredAsync(string configurationKey, Func<Task> configure,
            CancellationToken cancellationToken)
        {
            if (MongoIndexClientConfigurations.Configurations.TryGetValue(configurationKey, out var configured))
                if (configured)
                    return;

            using (await new AsyncDuplicateLock()
                .LockAsync(EntityConfiguration.CollectionName, cancellationToken)
                .ConfigureAwait(false))
            {
                if (MongoIndexClientConfigurations.Configurations.TryGetValue(configurationKey, out configured))
                    if (configured)
                        return;

                await configure();

                MongoIndexClientConfigurations.Configurations[configurationKey] = true;
            }
        }

        protected override Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<Expression<Func<TEntity, bool>>>());
        }
    }
}