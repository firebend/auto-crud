using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit;

public class MassTransitDomainEventPublisher<TKey, TEntity> : IEntityDomainEventPublisher<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IBus _bus;

    public MassTransitDomainEventPublisher(IBus bus)
    {
        _bus = bus;
    }

    public Task PublishEntityAddEventAsync(EntityAddedDomainEvent<TEntity> domainEvent,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
        => PublishAsync(domainEvent, transaction, cancellationToken);

    public Task PublishEntityDeleteEventAsync(EntityDeletedDomainEvent<TEntity> domainEvent,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
        => PublishAsync(domainEvent, transaction, cancellationToken);

    public Task PublishEntityUpdatedEventAsync(EntityUpdatedDomainEvent<TEntity> domainEvent,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
        => PublishAsync(domainEvent, transaction, cancellationToken);

    private Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        if (transaction == null)
        {
            return _bus.Publish(domainEvent, cancellationToken);
        }

        return transaction.AddFunctionEnrollmentAsync(ct =>
            _bus.Publish(domainEvent, ct), cancellationToken);
    }
}
