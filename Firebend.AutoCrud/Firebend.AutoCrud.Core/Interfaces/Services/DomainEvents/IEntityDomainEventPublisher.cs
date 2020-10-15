using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IDomainEventPublisher
    {
        Task PublishEntityAddEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken);
        
        Task PublishEntityDeleteEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken);
    }
}