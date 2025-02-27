using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;

public interface IEntityAddedDomainEventSubscriber<TEntity> : IDomainEventSubscriber
    where TEntity : class
{
    public Task EntityAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken);
}
