using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsCreateService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsCreateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly IDomainEventContextProvider _domainEventContextProvider;
    private readonly IEntityDomainEventPublisher _domainEventPublisher;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly bool _isDefaultPublisher;

    public AbstractMongoCustomFieldsCreateService(IMongoClient client,
        ILogger<AbstractMongoCustomFieldsCreateService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService,
        IDomainEventContextProvider domainEventContextProvider,
        IEntityDomainEventPublisher domainEventPublisher,
        ISessionTransactionManager transactionManager) : base(client, logger, entityConfiguration, mongoRetryService)
    {
        _domainEventContextProvider = domainEventContextProvider;
        _domainEventPublisher = domainEventPublisher;
        _transactionManager = transactionManager;
        _isDefaultPublisher = domainEventPublisher is DefaultEntityDomainEventPublisher;
    }

    public async Task<CustomFieldsEntity<TKey>>
        CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField,
            CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await CreateAsync(rootEntityKey, customField, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        if (customField.Id == default)
        {
            customField.Id = MongoIdGeneratorComb.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);
        }

        customField.CreatedDate = DateTimeOffset.UtcNow;
        customField.ModifiedDate = DateTimeOffset.UtcNow;

        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters);
        var mongoCollection = GetCollection();
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

    private Task PublishUpdatedDomainEventAsync(TEntity previous,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        if (_domainEventPublisher == null || _isDefaultPublisher)
        {
            return Task.CompletedTask;
        }

        var domainEvent = new EntityUpdatedDomainEvent<TEntity>
        {
            Previous = previous,
            OperationsJson =
                JsonConvert.SerializeObject(patch?.Operations, Formatting.None,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }),
            EventContext = _domainEventContextProvider?.GetContext()
        };

        return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);
    }
}
