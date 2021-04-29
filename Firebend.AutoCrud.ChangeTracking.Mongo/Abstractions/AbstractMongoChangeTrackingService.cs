using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.ChangeTracking.Mongo.Abstractions
{
    public abstract class AbstractMongoChangeTrackingService<TEntityKey, TEntity> :
        BaseDisposable,
        IChangeTrackingService<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {
        private readonly IMongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> _createClient;

        protected AbstractMongoChangeTrackingService(IMongoCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>> createClient)
        {
            _createClient = createClient;
        }

        public Task TrackAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _createClient.CreateAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Added",
                    domainEvent.Entity,
                    domainEvent.Entity.Id),
                cancellationToken);

        public Task TrackDeleteAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _createClient.CreateAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Delete",
                    domainEvent.Entity,
                    domainEvent.Entity.Id),
                cancellationToken);

        public Task TrackUpdateAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _createClient.CreateAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Update",
                    domainEvent.Previous,
                    domainEvent.Previous.Id,
                    domainEvent.Patch),
                cancellationToken);

        private static ChangeTrackingEntity<TEntityKey, TEntity> GetChangeTrackingEntityBase(DomainEventBase domainEvent,
            string action,
            TEntity entity,
            TEntityKey id,
            JsonPatchDocument<TEntity> patchDocument = null)
            => new()
        {
            ModifiedDate = domainEvent.Time,
            Source = domainEvent.EventContext?.Source,
            UserEmail = domainEvent.EventContext?.UserEmail,
            Action = action,
            Changes = patchDocument?.Operations,
            Entity = entity,
            EntityId = id,
            DomainEventCustomContext = domainEvent.EventContext?.CustomContext
        };
    }
}
