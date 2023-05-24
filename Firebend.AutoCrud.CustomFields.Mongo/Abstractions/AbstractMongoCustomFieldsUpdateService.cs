using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsUpdateService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{

    private readonly IDomainEventContextProvider _domainEventContextProvider;
    private readonly IEntityDomainEventPublisher<TKey, TEntity> _domainEventPublisher;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly bool _hasPublisher;

    private const string CustomFieldsName = nameof(ICustomFieldsEntity<Guid>.CustomFields);
    private const string ArrayDefFieldName = "customField";
    private const string ArrayFilterDefId = $"{ArrayDefFieldName}._id";

    public AbstractMongoCustomFieldsUpdateService(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<AbstractMongoCustomFieldsUpdateService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService,
        IDomainEventContextProvider domainEventContextProvider,
        IEntityDomainEventPublisher<TKey, TEntity> domainEventPublisher,
        ISessionTransactionManager transactionManager) : base(clientFactory, logger, entityConfiguration, mongoRetryService)
    {
        _domainEventContextProvider = domainEventContextProvider;
        _domainEventPublisher = domainEventPublisher;
        _transactionManager = transactionManager;
        _hasPublisher = domainEventPublisher is not null and not DefaultEntityDomainEventPublisher<TKey, TEntity>;
    }

    public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await UpdateAsync(rootEntityKey, customField, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        customField.ModifiedDate = DateTimeOffset.UtcNow;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == customField.Id);

        var mongoCollection = await GetCollectionAsync();
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

        var patch = new JsonPatchDocument<TEntity>();

        if ((result.CustomFields?.Count ?? 0) <= 0)
        {
            patch.Replace(x => x.CustomFields, new List<CustomFieldsEntity<TKey>> { customField });
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
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await PatchAsync(rootEntityKey, key, jsonPatchDocument, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
        Guid key,
        JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = await GetCollectionAsync();

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
                    ArrayFilters = new[] { arrayFilters },
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
                    ArrayFilters = new[] { arrayFilters },
                },
                cancellationToken);
        }

        if (result is null)
        {
            return null;
        }

        var entityPatch = new JsonPatchDocument<TEntity>();
        var entity = result.Clone();
        var index = entity.CustomFields.FindIndex(x => x.Id == key);

        if (entity.CustomFields?.Count <= 0)
        {
            entityPatch.Replace(x => x.CustomFields, new List<CustomFieldsEntity<TKey>> { entity.CustomFields[index] });
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

    private static string FixMongoPatchPath(Operation<CustomFieldsEntity<TKey>> operation)
        => operation.path[1..].FirstCharToLower();

    private Task PublishUpdatedDomainEventAsync(TEntity previous,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        if (_hasPublisher is false)
        {
            return Task.CompletedTask;
        }

        var domainEvent = new EntityUpdatedDomainEvent<TEntity>
        {
            Previous = previous,
            OperationsJson = JsonConvert.SerializeObject(patch?.Operations, Formatting.None, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }),
            EventContext = _domainEventContextProvider?.GetContext()
        };

        return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);

    }
}
