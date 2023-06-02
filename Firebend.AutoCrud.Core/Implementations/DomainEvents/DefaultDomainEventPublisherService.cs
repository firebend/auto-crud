using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Ids;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.JsonPatch.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Implementations.DomainEvents;

public class DefaultDomainEventPublisherService<TKey, TEntity> : IDomainEventPublisherService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IEntityDomainEventPublisher<TKey, TEntity> _publisher;
    private readonly IDomainEventContextProvider _contextProvider;
    private readonly IEntityReadService<TKey, TEntity> _readService;
    private readonly IJsonPatchGenerator _patchGenerator;
    private readonly bool _hasPublisher;
    private readonly bool _hasDomainEventContextProvider;

    public DefaultDomainEventPublisherService(
        IEntityReadService<TKey, TEntity> readService,
        IJsonPatchGenerator patchGenerator,
        IEntityDomainEventPublisher<TKey, TEntity> publisher = null,
        IDomainEventContextProvider contextProvider = null)
    {
        _publisher = publisher;
        _contextProvider = contextProvider;
        _readService = readService;
        _patchGenerator = patchGenerator;
        _hasPublisher = publisher is not null and not DefaultEntityDomainEventPublisher<TKey, TEntity>;
        _hasDomainEventContextProvider = contextProvider is not null and not DefaultDomainEventContextProvider;
    }

    public async Task<TEntity> ReadAndPublishAddedEventAsync(TKey key, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        var entity = await _readService.GetByKeyAsync(key, cancellationToken);

        if (!_hasPublisher)
        {
            return entity;
        }

        var message = new EntityAddedDomainEvent<TEntity>
        {
            Entity = entity,
            Time = entity is IModifiedEntity modifiedEntity ? modifiedEntity.CreatedDate : DateTimeOffset.UtcNow,
            EventContext = _hasDomainEventContextProvider ? _contextProvider.GetContext() : null,
            MessageId = CombGuid.New()
        };

        await _publisher.PublishEntityAddEventAsync(message, transaction, cancellationToken);

        return entity;
    }

    protected virtual async Task<TEntity> ReadAndPublishUpdateEventInternal(TKey key,
        TEntity previous,
        IEntityTransaction transaction,
        JsonPatchDocument<TEntity> patch,
        Func<TEntity, TEntity, JsonPatchDocument<TEntity>> patchGenerator,
        CancellationToken cancellationToken)
    {
        var entity = await _readService.GetByKeyAsync(key, cancellationToken);

        if (!_hasPublisher)
        {
            return entity;
        }

        if (patch is null && patchGenerator is not null)
        {
            patch = patchGenerator(previous, entity);
        }

        var message = new EntityUpdatedDomainEvent<TEntity>
        {
            Previous = previous,
            Operations = patch?.Operations,
            Time = entity is IModifiedEntity modifiedEntity ? modifiedEntity.ModifiedDate : DateTimeOffset.UtcNow,
            EventContext = _hasDomainEventContextProvider ? _contextProvider.GetContext() : null,
            MessageId = CombGuid.New(),
        };

        await _publisher.PublishEntityUpdatedEventAsync(message, transaction, cancellationToken);

        return entity;
    }

    public Task<TEntity> ReadAndPublishUpdateEventAsync(TKey key,
        TEntity previous,
        IEntityTransaction transaction,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken)
        => ReadAndPublishUpdateEventInternal(key,
            previous,
            transaction,
            patch,
            null,
            cancellationToken);

    public Task<TEntity> ReadAndPublishUpdateEventAsync(TKey key, TEntity previous, IEntityTransaction transaction, CancellationToken cancellationToken)
        => ReadAndPublishUpdateEventInternal(key,
            previous,
            transaction,
            null,
            PatchGenerator,
            cancellationToken);

    private JsonPatchDocument<TEntity> PatchGenerator(TEntity prev, TEntity curr)
    {
        var patch = _patchGenerator.Generate(prev, curr);
        return patch;
    }

    public async Task PublishDeleteEventAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        if (!_hasPublisher)
        {
            return;
        }

        var message = new EntityDeletedDomainEvent<TEntity>
        {
            Time = entity is IModifiedEntity modifiedEntity ? modifiedEntity.ModifiedDate : DateTimeOffset.UtcNow,
            EventContext = _hasDomainEventContextProvider ? _contextProvider.GetContext() : null,
            MessageId = CombGuid.New(),
            Entity = entity
        };

        await _publisher.PublishEntityDeleteEventAsync(message, transaction, cancellationToken);
    }

}
