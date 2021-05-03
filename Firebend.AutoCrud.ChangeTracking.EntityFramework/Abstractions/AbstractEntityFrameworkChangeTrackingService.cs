using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    public abstract class AbstractEntityFrameworkChangeTrackingService<TEntityKey, TEntity> :
        EntityFrameworkCreateClient<Guid, ChangeTrackingEntity<TEntityKey, TEntity>>,
        IChangeTrackingService<TEntityKey, TEntity>
        where TEntityKey : struct
        where TEntity : class, IEntity<TEntityKey>
    {
        private readonly IChangeTrackingOptionsProvider<TEntityKey, TEntity> _changeTrackingOptionsProvider;

        protected AbstractEntityFrameworkChangeTrackingService(
            IChangeTrackingDbContextProvider<TEntityKey, TEntity> provider,
            IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider) :
            base(provider, null, null, null)
        {
            _changeTrackingOptionsProvider = changeTrackingOptionsProvider;
        }

        public Task TrackAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => AddAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Added",
                    domainEvent.Entity,
                    domainEvent.Entity.Id),
                cancellationToken);

        public Task TrackDeleteAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => AddAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Delete",
                    domainEvent.Entity,
                    domainEvent.Entity.Id),
                cancellationToken);

        public Task TrackUpdateAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => AddAsync(
                GetChangeTrackingEntityBase(domainEvent,
                    "Update",
                    domainEvent.Previous,
                    domainEvent.Previous.Id,
                    domainEvent.Patch),
                cancellationToken);

        private ChangeTrackingEntity<TEntityKey, TEntity> GetChangeTrackingEntityBase(DomainEventBase domainEvent,
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
            DomainEventCustomContext = _changeTrackingOptionsProvider?.Options?.PersistCustomContext ?? false
                    ? domainEvent.EventContext?.CustomContext
                    : null
        };
    }
}
