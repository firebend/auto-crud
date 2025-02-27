using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

public interface IEntityDomainEventPublisher<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Task PublishEntityAddEventAsync(EntityAddedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task PublishEntityDeleteEventAsync(EntityDeletedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task PublishEntityUpdatedEventAsync(EntityUpdatedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
