using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityDomainEventPublisher
    {
        Task PublishEntityAddEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default);
        
        Task PublishEntityDeleteEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default);
        
        Task PublishEntityUpdatedEventAsync<TEntity>(TEntity original, TEntity modified, CancellationToken cancellationToken = default);
    }
}