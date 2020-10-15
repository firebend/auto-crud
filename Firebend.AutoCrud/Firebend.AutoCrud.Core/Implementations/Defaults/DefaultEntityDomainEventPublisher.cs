using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityDomainEventPublisher : IEntityDomainEventPublisher
    {
        public Task PublishEntityAddEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishEntityDeleteEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task PublishEntityUpdatedEventAsync<TEntity>(TEntity original, TEntity modified, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}