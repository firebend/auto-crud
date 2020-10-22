using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityDomainEventPublisher
    {
        Task PublishEntityAddEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class;

        Task PublishEntityDeleteEventAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class;

        Task PublishEntityUpdatedEventAsync<TEntity>(TEntity original, TEntity modified, CancellationToken cancellationToken = default)
            where TEntity : class;
    }
}