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
public class ChangeTrackingDeleteDomainEventHandler<TKey, TEntity> :
    BaseDisposable,
    IEntityDeletedDomainEventSubscriber<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IChangeTrackingService<TKey, TEntity> _changeTrackingService;
    private readonly ILogger<ChangeTrackingDeleteDomainEventHandler<TKey, TEntity>> _logger;

    public ChangeTrackingDeleteDomainEventHandler(IChangeTrackingService<TKey, TEntity> changeTrackingService, ILogger<ChangeTrackingDeleteDomainEventHandler<TKey, TEntity>> logger)
    {
        _changeTrackingService = changeTrackingService;
        _logger = logger;
    }

    public Task EntityDeletedAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Handling deleted event for entity of type {EntityType} with key {Key}.", typeof(TEntity).Name, domainEvent.Entity.Id);
        return _changeTrackingService.TrackDeleteAsync(domainEvent, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _changeTrackingService?.Dispose();
}
