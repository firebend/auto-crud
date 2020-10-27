using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Abstractions
{
    public abstract class AbstractChangeTrackingAddedDomainEventHandler<TKey, TEntity> :
        IEntityAddedDomainEventSubscriber<TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;

        protected AbstractChangeTrackingAddedDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService)
        {
            _changeTrackingService = changeTrackingService;
        }

        public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            => _changeTrackingService.TrackAddedAsync(domainEvent, cancellationToken);
    }
}