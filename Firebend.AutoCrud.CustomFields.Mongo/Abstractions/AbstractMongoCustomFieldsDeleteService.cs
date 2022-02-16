using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsDeleteService<TKey, TEntity> :
    MongoClientBaseEntity<TKey, TEntity>,
    ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{

    private readonly IDomainEventContextProvider _domainEventContextProvider;
    private readonly IEntityDomainEventPublisher _domainEventPublisher;
    private readonly bool _isDefaultPublisher;

    public AbstractMongoCustomFieldsDeleteService(IMongoClient client,
        ILogger<AbstractMongoCustomFieldsDeleteService<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService,
        IDomainEventContextProvider domainEventContextProvider,
        IEntityDomainEventPublisher domainEventPublisher) : base(client, logger, entityConfiguration, mongoRetryService)
    {
        _domainEventContextProvider = domainEventContextProvider;
        _domainEventPublisher = domainEventPublisher;
        _isDefaultPublisher = domainEventPublisher is DefaultEntityDomainEventPublisher;
    }

    public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key, CancellationToken cancellationToken = default)
        => DeleteAsync(rootEntityKey, key, null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        var filters = await BuildFiltersAsync(x => x.Id.Equals(rootEntityKey), cancellationToken).ConfigureAwait(false);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters)
                                & Builders<TEntity>.Filter.ElemMatch(x => x.CustomFields, cf => cf.Id == key);

        var mongoCollection = GetCollection();
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

        //todo: pub domain event

        var patch = new JsonPatchDocument<TEntity>();
        patch.Remove(x => x.CustomFields, result.CustomFields.FindIndex(x => x.Id == key));
        await PublishUpdatedDomainEventAsync(result, patch, entityTransaction, cancellationToken);

        return result.CustomFields?.FirstOrDefault(x => x.Id == key);
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
            OperationsJson = JsonConvert.SerializeObject(patch?.Operations, Formatting.None, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }),
            EventContext = _domainEventContextProvider?.GetContext()
        };

        return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);

    }
}
