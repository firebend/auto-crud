using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Extensions;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;

public class MongoChangeTrackingService<TEntityKey, TEntity> :
    BaseDisposable,
    IChangeTrackingService<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    private readonly IMongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _createClient;

    public MongoChangeTrackingService(IMongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> createClient)
    {
        _createClient = createClient;
    }

    public Task TrackAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
        => _createClient.CreateAsync(
            GetChangeTrackingEntityBase(domainEvent,
                "Added",
                domainEvent.Entity,
                domainEvent.Entity.Id),
            cancellationToken);

    public Task TrackDeleteAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
        => _createClient.CreateAsync(
            GetChangeTrackingEntityBase(domainEvent,
                "Delete",
                domainEvent.Entity,
                domainEvent.Entity.Id),
            cancellationToken);

    public Task TrackUpdateAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
        => _createClient.CreateAsync(
            GetChangeTrackingEntityBase(domainEvent,
                "Update",
                domainEvent.Previous,
                domainEvent.Previous.Id,
                domainEvent.Operations),
            cancellationToken);

    private ChangeTrackingEntity<TEntityKey, TEntity> GetChangeTrackingEntityBase(DomainEventBase domainEvent,
        string action,
        TEntity entity,
        TEntityKey id,
        List<Operation<TEntity>> operations = null)
    {
        var changeEntity = new ChangeTrackingEntity<TEntityKey, TEntity>
        {
            ModifiedDate = domainEvent.Time,
            Source = domainEvent.EventContext?.Source,
            UserEmail = domainEvent.EventContext?.UserEmail,
            Action = action,
            Changes = operations.ToNonGenericOperations(),
            Entity = entity,
            EntityId = id,
            DomainEventCustomContext = domainEvent.EventContext?.CustomContext
        };

        return changeEntity;
    }
}
