using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsUpdateService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{

    private static readonly string CustomFieldsName = nameof(ICustomFieldsEntity<Guid>.CustomFields);
    private const string ArrayDefFieldName = "customField";
    private const string ArrayFilterDefId = $"{ArrayDefFieldName}._id";

    public AbstractMongoCustomFieldsUpdateService(IMongoClient client,
        ILogger<AbstractMongoCustomFieldsUpdateService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService) : base(client, logger, entityConfiguration, mongoRetryService)
    {
    }

    public Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField, CancellationToken cancellationToken = default)
        => UpdateAsync(rootEntityKey, customField, null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        customField.ModifiedDate = DateTimeOffset.UtcNow;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == customField.Id);

        var mongoCollection = GetCollection();
        var updateDefinition = Builders<TEntity>.Update.Set(x => x.CustomFields[-1], customField);

        if (typeof(IModifiedEntity).IsAssignableFrom(typeof(TEntity)))
        {
            updateDefinition = Builders<TEntity>.Update.Combine(updateDefinition,
                Builders<TEntity>.Update.Set(nameof(IModifiedEntity.ModifiedDate), DateTimeOffset.UtcNow));
        }

        var session = UnwrapSession(entityTransaction);

        TEntity result;

        if (session is not null)
        {
            result = await mongoCollection.FindOneAndUpdateAsync(session,
                filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.After,
                },
                cancellationToken);
        }

        return result?.CustomFields?.FirstOrDefault(x => x.Id == customField.Id);
    }

    public Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
        Guid key,
        JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
        CancellationToken cancellationToken = default)
        => PatchAsync(rootEntityKey, key, jsonPatchDocument,null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
        Guid key,
        JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = GetCollection();

        var list = jsonPatchDocument
            .Operations
            .Select(operation => new { operation, field = FixMongoPatchPath(operation) })
            .Select(x => new { patchDocument = x, fieldPath = $"{CustomFieldsName}.$[{ArrayDefFieldName}].{x.field}" })
            .Select(x => Builders<TEntity>.Update.Set(x.fieldPath, x.patchDocument.operation.value));

        var updateDefinition = Builders<TEntity>.Update.Combine(list);

        if (typeof(IModifiedEntity).IsAssignableFrom(typeof(TEntity)))
        {
            updateDefinition = Builders<TEntity>.Update.Combine(updateDefinition,
                Builders<TEntity>.Update.Set(nameof(IModifiedEntity.ModifiedDate), DateTimeOffset.UtcNow));
        }

        var arrayFilters = new BsonDocumentArrayFilterDefinition<BsonDocument>(
            new BsonDocument(ArrayFilterDefId, key.ToString()));

        var session = UnwrapSession(entityTransaction);

        TEntity result;

        if (session is not null)
        {
            result = await mongoCollection.FindOneAndUpdateAsync(session,
                filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.After,
                    ArrayFilters = new [] { arrayFilters},
                },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.After,
                    ArrayFilters = new [] { arrayFilters},
                },
                cancellationToken);
        }

        return result?.CustomFields?.FirstOrDefault(x => x.Id == key);
    }

    private static string FixMongoPatchPath(Operation<CustomFieldsEntity<TKey>> operation)
        => operation.path.Substring(1, operation.path.Length - 1).FirstCharToLower();
}
