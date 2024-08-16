using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
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

public class MongoCustomFieldsDeleteService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{

    private readonly IDomainEventContextProvider _domainEventContextProvider;
    private readonly IEntityDomainEventPublisher<TKey, TEntity> _domainEventPublisher;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly bool _hasPublisher;

    public MongoCustomFieldsDeleteService(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoCustomFieldsDeleteService<TKey, TEntity>> logger,
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

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key, CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteAsync(rootEntityKey, key, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = await GetCollectionAsync(null, cancellationToken);
        var updateDefinition = Builders<TEntity>.Update.PullFilter(x => x.CustomFields, x => x.Id == key);


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

        var patch = new JsonPatchDocument<TEntity>();
        patch.Remove(x => x.CustomFields, result.CustomFields.FindIndex(x => x.Id == key));
        await PublishUpdatedDomainEventAsync(result, patch, entityTransaction, cancellationToken);

        return result.CustomFields?.FirstOrDefault(x => x.Id == key);
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
            EventContext = _domainEventContextProvider?.GetContext()
        };

        return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);

    }
}
