using Firebend.AutoCrud.Caching.interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Caching.implementations;

public class DefaultEntityCacheService<TKey, TEntity>(
    IDistributedCache cache,
    IEntityCacheOptions entityCacheOptions,
    ILogger<DefaultEntityCacheService<TKey, TEntity>> logger) : IEntityCacheService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private string GetCacheKey(TKey key)
    {
        var cacheKey = entityCacheOptions.GetKey(key);
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentNullException(nameof(key), "Cache Key cannot be null or empty!");
        }
        return cacheKey;
    }

    public async Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(key);

        try
        {
            var serialized = await cache.GetStringAsync(cacheKey, cancellationToken);

            return string.IsNullOrWhiteSpace(serialized) ? null : entityCacheOptions.Serializer.Deserialize<TEntity>(serialized);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error getting cache key {CacheKey}", cacheKey);
            return null;
        }
    }

    public Task SetAsync(TEntity entity, CancellationToken cancellationToken) =>
        SetAsync(entity, entityCacheOptions.GetCacheEntryOptions(entity), cancellationToken);

    public async Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(entity.Id);
        var serialized = entityCacheOptions.Serializer.Serialize(entity);

        try
        {
            await cache.SetStringAsync(cacheKey, serialized, cacheOptions, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error setting cache key {CacheKey}", cacheKey);
        }
    }

    public async Task RemoveAsync(TKey key, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(key);
        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error removing cache key {CacheKey}", cacheKey);
        }
    }
}
