using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Abstractions;

[DisplayName("ChangeTracking")]
public abstract class AbstractChangeTrackingDeleteDomainEventHandler<TKey, TEntity> :
    BaseDisposable,
    IEntityDeletedDomainEventSubscriber<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;

    protected AbstractChangeTrackingDeleteDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService)
    {
        _changeTrackingService = changeTrackingService;
    }

    public Task EntityDeletedAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
        => _changeTrackingService.TrackDeleteAsync(domainEvent, cancellationToken);

    protected override void DisposeManagedObjects() => _changeTrackingService?.Dispose();
}
