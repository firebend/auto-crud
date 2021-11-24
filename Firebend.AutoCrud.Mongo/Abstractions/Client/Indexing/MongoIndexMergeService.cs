using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing
{
    public class MongoIndexMergeService : MongoClientBase, IMongoIndexMergeService
    {
        private readonly IMongoIndexComparisonService _comparisonService;
        private readonly ILogger<MongoIndexMergeService> _logger;

        public MongoIndexMergeService(IMongoClient client,
            ILogger<MongoIndexMergeService> logger,
            IMongoRetryService mongoRetryService,
            IMongoIndexComparisonService comparisonService) : base(client, logger, mongoRetryService)
        {
            _logger = logger;
            _comparisonService = comparisonService;
        }


        public async Task MergeIndexesAsync<TEntity>(IMongoCollection<TEntity> dbCollection,
            CreateIndexModel<TEntity>[] indexModels,
            CancellationToken cancellationToken)
        {
            if (!_comparisonService.EnsureUnique(dbCollection, indexModels))
            {
                throw new Exception($"Ensure index definitions are unique. Entity Type {typeof(TEntity)}");
            }

            var indexesCursor = await dbCollection
                .Indexes
                .ListAsync(cancellationToken)
                .ConfigureAwait(false);

            var indexes = await indexesCursor
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var hasExistingIndexes = indexes?.Any() ?? false;

            if (hasExistingIndexes)
            {
                await MergeIndexesAsync(dbCollection, indexModels, indexes, cancellationToken);
            }
            else
            {
                await CreateIndexesAsync(dbCollection, indexModels, cancellationToken);
            }
        }

        private async Task MergeIndexesAsync<TEntity>(IMongoCollection<TEntity> dbCollection,
            IEnumerable<CreateIndexModel<TEntity>> indexesToAdd,
            IReadOnlyCollection<BsonDocument> indexes,
            CancellationToken cancellationToken)
        {
            var adds = new List<CreateIndexModel<TEntity>>();
            var drops = new List<string>();

            foreach (var indexToAdd in indexesToAdd)
            {
                if (HandleTextIndex(dbCollection, indexes, indexToAdd, drops, adds))
                {
                    continue;
                }

                HandleOtherIndexes(dbCollection, indexes, indexToAdd, adds, drops);
            }

            if (drops.HasValues())
            {
                await DropIndexAsync(dbCollection, drops, cancellationToken);
            }

            if (adds.HasValues())
            {
                await CreateIndexesAsync(dbCollection, adds, cancellationToken);
            }
        }

        private Task CreateIndexesAsync<TEntity>(IMongoCollection<TEntity> dbCollection,
            IReadOnlyCollection<CreateIndexModel<TEntity>> adds,
            CancellationToken cancellationToken)
            => RetryErrorAsync(() => dbCollection.Indexes.CreateManyAsync(adds, cancellationToken));

        private async Task DropIndexAsync<TEntity>(IMongoCollection<TEntity> dbCollection, List<string> indexNames, CancellationToken cancellationToken)
        {
            foreach (var indexName in indexNames)
            {
                await RetryErrorAsync(() => dbCollection.Indexes.DropOneAsync(indexName, cancellationToken));
            }
        }

        private void HandleOtherIndexes<TEntity>(
            IMongoCollection<TEntity> mongoCollection,
            IEnumerable<BsonDocument> indexes,
            CreateIndexModel<TEntity> indexToAdd,
            ICollection<CreateIndexModel<TEntity>> adds,
            ICollection<string> drops)
        {
            if (indexToAdd is null)
            {
                return;
            }

            var indexName = indexToAdd.Options?.Name;

            BsonDocument existingIndex = null;

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                existingIndex = indexes.FirstOrDefault(x => x["name"].AsString.EqualsIgnoreCaseAndWhitespace(indexName));
            }

            if (existingIndex is null)
            {
                var keys = indexToAdd.Keys
                    .Render(mongoCollection.DocumentSerializer, new BsonSerializerRegistry())
                    .ToJson();

                existingIndex = indexes.FirstOrDefault(x => x["key"].ToJson().EqualsIgnoreCaseAndWhitespace(keys));
            }

            if (existingIndex == null)
            {
                adds.Add(indexToAdd);
                return;
            }

            if (_comparisonService.DoesIndexMatch(mongoCollection, existingIndex, indexToAdd))
            {
                return;
            }

            adds.Add(indexToAdd);
            drops.Add(existingIndex["name"].AsString);
        }

        private bool HandleTextIndex<TEntity>(
            IMongoCollection<TEntity> dbCollection,
            IEnumerable<BsonDocument> indexes,
            CreateIndexModel<TEntity> indexToAdd,
            ICollection<string> drops,
            ICollection<CreateIndexModel<TEntity>> adds)
        {
            var isIndexToAddTextIndex = indexToAdd
                .Keys
                .Render(dbCollection.DocumentSerializer, new BsonSerializerRegistry())
                .Where(x => x.Value.IsString)
                .Any(x => x.Value.AsString.EqualsIgnoreCaseAndWhitespace("text"));

            if (!isIndexToAddTextIndex)
            {
                return false;
            }

            var existingTextIndex = indexes.FirstOrDefault(y => y.Contains("textIndexVersion"));

            if (existingTextIndex == null)
            {
                adds.Add(indexToAdd);
                return true;
            }

            if (_comparisonService.DoesIndexMatch(dbCollection, existingTextIndex, indexToAdd))
            {
                return true;
            }

            drops.Add(existingTextIndex["name"].AsString);
            adds.Add(indexToAdd);

            return true;
        }
    }
}
