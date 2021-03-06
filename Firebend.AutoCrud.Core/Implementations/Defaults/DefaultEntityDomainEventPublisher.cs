using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public abstract class DefaultEntityDomainEventPublisher : IEntityDomainEventPublisher
    {
        public Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => Task.CompletedTask;

        public Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => Task.CompletedTask;

        public Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
            => Task.CompletedTask;
    }
}
