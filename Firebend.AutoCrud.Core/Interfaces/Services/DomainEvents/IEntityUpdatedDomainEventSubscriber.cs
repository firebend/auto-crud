using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityUpdatedDomainEventSubscriber<TEntity> : IDomainEventSubscriber
        where TEntity : class
    {
        Task EntityUpdatedAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default);
    }
}
