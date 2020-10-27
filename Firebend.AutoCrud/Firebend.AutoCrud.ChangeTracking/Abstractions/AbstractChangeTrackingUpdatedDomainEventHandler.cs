using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Abstractions
{
    public abstract class AbstractChangeTrackingUpdatedDomainEventHandler<TKey, TEntity> :
        IEntityUpdatedDomainEventSubscriber<TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;

        protected AbstractChangeTrackingUpdatedDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService)
        {
            _changeTrackingService = changeTrackingService;
        }

        public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _changeTrackingService.TrackUpdateAsync(domainEvent, cancellationToken);
    }
}