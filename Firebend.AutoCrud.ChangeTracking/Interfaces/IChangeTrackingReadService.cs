using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.ChangeTracking.Interfaces
{
    /// <summary>
    /// Encapsulates logic for reading change tracking events from a data store for a given entity.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    public interface IChangeTrackingReadService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Gets a <see cref="EntityPagedResponse{TEntity}"/> containing a paged set of changes.
        /// </summary>
        /// <param name="searchRequest">
        /// The <see cref="ChangeTrackingSearchRequest{TKey}"/> containing the parameters to search for changes by.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        /// A <see cref="EntityPagedResponse{TEntity}"/> containing a paged list of <see cref="ChangeTrackingEntity{TKey,TEntity}"/>
        /// </returns>
        Task<EntityPagedResponse<ChangeTrackingEntity<TKey, TEntity>>> GetChangesByEntityId(ChangeTrackingSearchRequest<TKey> searchRequest,
            CancellationToken cancellationToken = default);
    }
}
