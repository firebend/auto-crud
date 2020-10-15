using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityUpdatedDomainEventSubscriber<in TEntity>
    {
        Task EntityUpdatedAsync(TEntity original, TEntity modified, CancellationToken cancellationToken);
    }
}