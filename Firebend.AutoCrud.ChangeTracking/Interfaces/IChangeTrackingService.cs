using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;

namespace Firebend.AutoCrud.ChangeTracking.Interfaces
{
    /// <summary>
    /// Encapsulates logic for tracking Add, Update, Delete operations on entities.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of key the entity uses.
    /// </typeparam>
    /// <typeparam name="TEntity">
    /// The type of entity.
    /// </typeparam>
    public interface IChangeTrackingService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Tracks changes when an entity is added.
        /// </summary>
        /// <param name="domainEvent">
        /// The <see cref="EntityAddedDomainEvent{T}"/> containing information about an Entity Added event.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>
        /// </returns>
        Task TrackAddedAsync(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracks changes when an entity is deleted.
        /// </summary>
        /// <param name="domainEvent">
        /// The <see cref="EntityDeletedDomainEvent{T}"/> containing information about an Entity Deleted event.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>
        /// </returns>
        Task TrackDeleteAsync(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracks changes when an entity is updated.
        /// </summary>
        /// The <see cref="EntityUpdatedDomainEvent{T}"/> containing information about an Entity Updated event.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/>
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>
        /// </returns>
        Task TrackUpdateAsync(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default);
    }
}
