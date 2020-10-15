#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents
{
    public interface IEntityDeletedDomainEventSubscriber<in TEntity>
    {
        Task EntityDeletedAsync(TEntity entity, CancellationToken cancellationToken);
    }
}