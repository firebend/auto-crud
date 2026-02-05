using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.ChangeTracking.Implementations;

[DisplayName("ChangeTracking")]
public class ChangeTrackingAddedDomainEventHandler<TKey, TEntity> :
    BaseDisposable,
    IEntityAddedDomainEventSubscriber<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;
    private readonly ILogger<ChangeTrackingAddedDomainEventHandler<TKey, TEntity>> _logger;

    public ChangeTrackingAddedDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService, ILogger<ChangeTrackingAddedDomainEventHandler<TKey, TEntity>> logger)
    {
        _changeTrackingService = changeTrackingService;
        _logger = logger;
    }

    public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling added event for entity of type {EntityType} with key {Key}.", typeof(TEntity).Name, domainEvent.Entity.Id);
        return _changeTrackingService.TrackAddedAsync(domainEvent, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _changeTrackingService?.Dispose();
}
