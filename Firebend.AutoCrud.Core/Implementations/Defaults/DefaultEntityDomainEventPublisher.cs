using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public class DefaultEntityDomainEventPublisher<TKey, TEntity> : IEntityDomainEventPublisher<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task PublishEntityAddEventAsync(EntityAddedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task PublishEntityDeleteEventAsync(EntityDeletedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task PublishEntityUpdatedEventAsync(EntityUpdatedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => Task.CompletedTask;
}
