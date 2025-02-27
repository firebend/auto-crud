using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

public interface IEntityDeletedDomainEventSubscriber<TEntity> : IDomainEventSubscriber
    where TEntity : class
{
    public Task EntityDeletedAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken);
}
