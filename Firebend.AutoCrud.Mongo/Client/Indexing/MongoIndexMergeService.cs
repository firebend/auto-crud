using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Client.Indexing;

public class MongoIndexMergeService<TKey, TEntity> : MongoClientBase<TKey, TEntity>, IMongoIndexMergeService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IMongoIndexComparisonService _comparisonService;

    public MongoIndexMergeService(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoIndexMergeService<TKey, TEntity>> logger,
        IMongoRetryService mongoRetryService,
        IMongoIndexComparisonService comparisonService) : base(clientFactory, logger, mongoRetryService)
    {
        _comparisonService = comparisonService;
    }


    public async Task MergeIndexesAsync(IMongoCollection<TEntity> dbCollection,
        CreateIndexModel<TEntity>[] indexModels,
        CancellationToken cancellationToken)
    {
        if (!_comparisonService.EnsureUnique(dbCollection, indexModels))
        {
            throw new Exception($"Ensure index definitions are unique. Entity Type {typeof(TEntity)}");
        }

        var indexesCursor = await dbCollection.Indexes.ListAsync(cancellationToken);

        var indexes = await indexesCursor.ToListAsync(cancellationToken);

        var hasExistingIndexes = (indexes?.Count ?? 0) > 0;

        if (hasExistingIndexes)
        {
            await MergeIndexesAsync(dbCollection, indexModels, indexes, cancellationToken);
        }
        else
        {
            await CreateIndexesAsync(dbCollection, indexModels, cancellationToken);
        }
    }

    protected virtual async Task MergeIndexesAsync(IMongoCollection<TEntity> dbCollection,
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

    private Task CreateIndexesAsync(IMongoCollection<TEntity> dbCollection,
        IReadOnlyCollection<CreateIndexModel<TEntity>> adds,
        CancellationToken cancellationToken)
        => RetryErrorAsync(() => dbCollection.Indexes.CreateManyAsync(adds, cancellationToken));

    private async Task DropIndexAsync(IMongoCollection<TEntity> dbCollection, List<string> indexNames, CancellationToken cancellationToken)
    {
        foreach (var indexName in indexNames)
        {
            await RetryErrorAsync(() => dbCollection.Indexes.DropOneAsync(indexName, cancellationToken));
        }
    }

    private void HandleOtherIndexes(
        IMongoCollection<TEntity> mongoCollection,
        IReadOnlyCollection<BsonDocument> indexes,
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

    private bool HandleTextIndex(
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
