#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityUpdatedDomainEventSubscriber<in TEntity>
    {
        Task EntityUpdatedAsync(TEntity original, TEntity modified, CancellationToken cancellationToken);
    }
}