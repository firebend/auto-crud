using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Ids;
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
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.CustomFields.Mongo.Implementations;

public class MongoCustomFieldsCreateService<TKey, TEntity>(
    IMongoClientFactory<TKey, TEntity> clientFactory,
    ILogger<MongoCustomFieldsCreateService<TKey, TEntity>> logger,
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
        ICustomFieldsCreateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly bool _hasPublisher = domainEventPublisher is not null and not DefaultEntityDomainEventPublisher<TKey, TEntity>;

    public async Task<CustomFieldsEntity<TKey>>
        CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField,
            CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await CreateAsync(rootEntityKey, customField, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);

        if (customField.Id == default)
        {
            customField.Id = CombGuid.New();
        }

        customField.CreatedDate = DateTimeOffset.UtcNow;
        customField.ModifiedDate = DateTimeOffset.UtcNow;
        customField.EntityId = rootEntityKey;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters);
        var mongoCollection = await GetCollectionAsync(null, cancellationToken);
        var updateDefinition = Builders<TEntity>.Update.Push(x => x.CustomFields, customField);

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
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.Before },
                cancellationToken);
        }
        else
        {
            result = await mongoCollection.FindOneAndUpdateAsync(filtersDefinition,
                updateDefinition,
                new FindOneAndUpdateOptions<TEntity> { ReturnDocument = ReturnDocument.Before },
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
