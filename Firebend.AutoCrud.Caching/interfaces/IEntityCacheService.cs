using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Caching.interfaces;

public interface IEntityCacheService<in TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken);
    Task SetAsync(TEntity entity, CancellationToken cancellationToken);
    Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions, CancellationToken cancellationToken);
    Task RemoveAsync(TKey key, CancellationToken cancellationToken);
}
