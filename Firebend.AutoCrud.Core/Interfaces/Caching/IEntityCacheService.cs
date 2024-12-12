#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheService<in TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    /// <summary>
    /// Get an entity from the cache if it exists.
    /// </summary>
    /// <param name="key">The Id of the entity</param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>A task that gets TEntity from cache or null if cache not found.</returns>
    public Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set an entity in the cache using options provided by <see cref="Firebend.AutoCrud.Core.Interfaces.Caching.IEntityCacheOptions"/>.
    /// </summary>
    /// <param name="entity">Entity to cache</param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task SetAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set an entity in the cache with options.
    /// </summary>
    /// <param name="entity">Entity to cache</param>
    /// <param name="cacheOptions"><see cref="Firebend.AutoCrud.Core.Interfaces.Caching.IEntityCacheOptions"/></param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity from the cache.
    /// </summary>
    /// <param name="key">Id of the cached entity</param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task RemoveAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a collection of entities from the cache if it exists.
    /// </summary>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>A task that returns the list of TEntity from cache or null if cache not found.</returns>
    public Task<List<TEntity>?> GetCollectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a collection of entities in the cache using options provided by <see cref="Firebend.AutoCrud.Core.Interfaces.Caching.IEntityCacheOptions"/>.
    /// </summary>
    /// <param name="entities">The entities to cache</param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task SetCollectionAsync(List<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a collection of entities in the cache with options.
    /// </summary>
    /// <param name="entities">The entities to cache</param>
    /// <param name="cacheOptions"><see cref="Firebend.AutoCrud.Core.Interfaces.Caching.IEntityCacheOptions"/></param>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task SetCollectionAsync(List<TEntity> entities, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a collection of entities from the cache.
    /// </summary>
    /// <param name="cancellationToken">Optional. A <see cref="CancellationToken"/> to cancel the operation</param>
    /// <returns>The Task that represents the asynchronous operation.</returns>
    public Task RemoveCollectionAsync(CancellationToken cancellationToken = default);
}
