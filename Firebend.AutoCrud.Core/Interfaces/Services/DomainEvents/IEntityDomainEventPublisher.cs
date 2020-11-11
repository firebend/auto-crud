using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityDomainEventPublisher
    {
        Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class;

        Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class;

        Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class;
    }
}
