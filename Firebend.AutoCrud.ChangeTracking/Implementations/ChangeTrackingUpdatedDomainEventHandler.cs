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
public class ChangeTrackingUpdatedDomainEventHandler<TKey, TEntity> :
    BaseDisposable,
    IEntityUpdatedDomainEventSubscriber<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;
    private readonly ILogger<ChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>> _logger;

    public ChangeTrackingUpdatedDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService, ILogger<ChangeTrackingUpdatedDomainEventHandler<TKey, TEntity>> logger)
    {
        _changeTrackingService = changeTrackingService;
        _logger = logger;
    }

    public Task EntityUpdatedAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling updated event for entity of type {EntityType} with key {Key}.", typeof(TEntity).Name, domainEvent.Modified.Id);
        return _changeTrackingService.TrackUpdateAsync(domainEvent, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _changeTrackingService?.Dispose();
}
