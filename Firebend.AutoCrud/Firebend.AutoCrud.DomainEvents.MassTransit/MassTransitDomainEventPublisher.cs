using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit
{
    public class MassTransitDomainEventPublisher : IEntityDomainEventPublisher
    {
        private readonly IBus _bus;

        public MassTransitDomainEventPublisher(IBus bus)
        {
            _bus = bus;
        }

        public Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(domainEvent, cancellationToken);

        public Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(domainEvent, cancellationToken);

        public Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
            => _bus.Publish(domainEvent, cancellationToken);
    }
}