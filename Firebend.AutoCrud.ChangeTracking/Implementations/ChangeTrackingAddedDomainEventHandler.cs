using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Implementations;

[DisplayName("ChangeTracking")]
public class ChangeTrackingAddedDomainEventHandler<TKey, TEntity> :
    BaseDisposable,
    IEntityAddedDomainEventSubscriber<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;

    public ChangeTrackingAddedDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService)
    {
        _changeTrackingService = changeTrackingService;
    }

    public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
        => _changeTrackingService.TrackAddedAsync(domainEvent, cancellationToken);

    protected override void DisposeManagedObjects() => _changeTrackingService?.Dispose();
}
