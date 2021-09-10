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

        public MongoIndexMergeService(IMongoClient client,
            ILogger<MongoIndexMergeService> logger,
            IMongoRetryService mongoRetryService,
            IMongoIndexComparisonService comparisonService) : base(client, logger, mongoRetryService)
        {
            _comparisonService = comparisonService;
        }


        public async Task MergeIndexesAsync<TEntity>(IMongoCollection<TEntity> dbCollection,
            CreateIndexModel<TEntity>[] indexModels,
            CancellationToken cancellationToken)
        {
            if (!_comparisonService.EnsureUnique(indexModels))
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
                await RetryErrorAsync(() => dbCollection
                        .Indexes
                        .CreateManyAsync(indexModels, cancellationToken))
                    .ConfigureAwait(false);
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
                foreach (var d in drops)
                {
                    await dbCollection.Indexes.DropOneAsync(d, cancellationToken).ConfigureAwait(false);
                }
            }

            if (adds.HasValues())
            {
                await dbCollection.Indexes.CreateManyAsync(adds, cancellationToken).ConfigureAwait(false);
            }
        }

        private void HandleOtherIndexes<TEntity>(
            IMongoCollection<TEntity> mongoCollection,
            IEnumerable<BsonDocument> indexes,
            CreateIndexModel<TEntity> indexToAdd,
            ICollection<CreateIndexModel<TEntity>> adds,
            ICollection<string> drops)
        {
            var indexName = indexToAdd.Options.Name;
            var existingIndex = indexes.FirstOrDefault(x => x["name"].AsString.EqualsIgnoreCaseAndWhitespace(indexName));

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
