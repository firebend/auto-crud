using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using MassTransit;

namespace Firebend.AutoCrud.DomainEvents.MassTransit
{
    public abstract class MassTransitDomainEventPublisher : IEntityDomainEventPublisher
    {
        private readonly IBus _bus;

        protected MassTransitDomainEventPublisher(IBus bus)
        {
            _bus = bus;
        }

        public Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => PublishAsync(domainEvent, transaction, cancellationToken);

        public Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => PublishAsync(domainEvent, transaction, cancellationToken);

        public Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => PublishAsync(domainEvent, transaction, cancellationToken);

        private Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, IEntityTransaction transaction, CancellationToken cancellationToken)
        {
            if (transaction == null)
            {
                return _bus.Publish(domainEvent, cancellationToken);
            }

            return transaction.AddFunctionEnrollmentAsync(ct =>
                _bus.Publish(domainEvent, cancellationToken), cancellationToken);
        }
    }
}
