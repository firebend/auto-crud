using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityAddedDomainEventSubscriber<in TEntity>
    {
        Task EntityAddedAsync(TEntity entity, CancellationToken cancellationToken);
    }
}