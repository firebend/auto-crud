using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.ChangeTracking.Interfaces
{
    public interface IChangeTrackingReadService<TKey, TEntity>
        where TKey: struct
        where TEntity: class, IEntity<TKey>
    {
        Task<EntityPagedResponse<ChangeTrackingEntity<TKey, TEntity>>> GetChangesByEntityId(ChangeTrackingSearchRequest searchRequest,
            CancellationToken cancellationToken = default);
    }
}