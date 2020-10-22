using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Abstractions
{
    public class AbstractChangeTrackingDomainEventHandler<TKey, TEntity> :
        IEntityDeletedDomainEventSubscriber<TEntity>,
        IEntityUpdatedDomainEventSubscriber<TEntity>,
        IEntityAddedDomainEventSubscriber<TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;

        public AbstractChangeTrackingDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService)
        {
            _changeTrackingService = changeTrackingService;
        }

        public Task EntityDeletedAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _changeTrackingService.TrackDeleteAsync(domainEvent, cancellationToken);

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _changeTrackingService.TrackUpdateAsync(domainEvent, cancellationToken);

        public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _changeTrackingService.TrackAddedAsync(domainEvent, cancellationToken);
    }
}