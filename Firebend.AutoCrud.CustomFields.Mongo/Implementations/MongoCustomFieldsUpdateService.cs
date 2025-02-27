using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.CustomFields.Mongo.Implementations;

public class MongoCustomFieldsUpdateService<TKey, TEntity>(
    IMongoClientFactory<TKey, TEntity> clientFactory,
    ILogger<MongoCustomFieldsUpdateService<TKey, TEntity>> logger,
    IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
    IMongoRetryService mongoRetryService,
    IDomainEventContextProvider domainEventContextProvider,
    IEntityDomainEventPublisher<TKey, TEntity> domainEventPublisher,
    ISessionTransactionManager transactionManager,
    IMongoReadPreferenceService readPreferenceService,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    :
        MongoClientBaseEntity<TKey, TEntity>(clientFactory,
            logger,
            entityConfiguration,
            mongoRetryService,
            readPreferenceService),
        ICustomFieldsUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly bool _hasPublisher = domainEventPublisher is not null and not DefaultEntityDomainEventPublisher<TKey, TEntity>;

    private const string CustomFieldsName = nameof(ICustomFieldsEntity<Guid>.CustomFields);
    private const string ArrayDefFieldName = "customField";
    private const string ArrayFilterDefId = $"{ArrayDefFieldName}._id";

    public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField, CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await UpdateAsync(rootEntityKey, customField, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        customField.ModifiedDate = DateTimeOffset.UtcNow;
        customField.EntityId = rootEntityKey;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == customField.Id);

        var mongoCollection = await GetCollectionAsync(null, cancellationToken);

        var updateDefinition = Builders<TEntity>.Update.Set(x => x.CustomFields.FirstMatchingElement(), customField);

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
                    ReturnDocument = ReturnDocument.Before
                },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.Before,
                },
                cancellationToken);
        }

        if (result is null)
        {
            return null;
        }

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(rootEntityKey, cancellationToken);
        }

        var patch = new JsonPatchDocument<TEntity>();

        if ((result.CustomFields?.Count ?? 0) <= 0)
        {
            patch.Replace(x => x.CustomFields, [customField]);
        }
        else
        {
            patch.Add(x => x.CustomFields, customField);
        }


        await PublishUpdatedDomainEventAsync(result, patch, entityTransaction, cancellationToken);
        return customField;
    }

    public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
        Guid key,
        JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await PatchAsync(rootEntityKey, key, jsonPatchDocument, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
        Guid key,
        JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = await GetCollectionAsync(null, cancellationToken);

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
                    ReturnDocument = ReturnDocument.Before,
                    ArrayFilters = [arrayFilters],
                },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity>
                {
                    ReturnDocument = ReturnDocument.Before,
                    ArrayFilters = [arrayFilters],
                },
                cancellationToken);
        }

        if (result is null)
        {
            return null;
        }

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(rootEntityKey, cancellationToken);
        }

        var entityPatch = new JsonPatchDocument<TEntity>();
        var entity = result.Clone();
        var index = entity.CustomFields.FindIndex(x => x.Id == key);

        if (entity.CustomFields?.Count <= 0)
        {
            entityPatch.Replace(x => x.CustomFields, [entity.CustomFields[index]]);
        }
        else
        {
            foreach (var operation in jsonPatchDocument.Operations)
            {
                var path = $"/{CustomFieldsName}/{index}{operation.path}";
                entityPatch.Operations.Add(new Operation<TEntity>(operation.op, path, operation.from, operation.value));
            }
        }

        entityPatch.ApplyTo(entity);
        await PublishUpdatedDomainEventAsync(result, entityPatch, entityTransaction, cancellationToken);
        return entity.CustomFields?.FirstOrDefault(x => x.Id == key);
    }

    private static string FixMongoPatchPath(OperationBase operation)
        => operation.path[1..].FirstCharToLower();

    private Task PublishUpdatedDomainEventAsync(TEntity previous,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        if (_hasPublisher is false)
        {
            return Task.CompletedTask;
        }

        var domainEvent = new EntityUpdatedDomainEvent<TEntity>
        {
            Previous = previous,
            Operations = patch?.Operations,
            EventContext = domainEventContextProvider?.GetContext()
        };

        return domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);
    }
}
