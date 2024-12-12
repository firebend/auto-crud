#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

/// <summary>
/// Default implementation of IEntityCacheService. This service is used to cache entities and collections of entities and
/// to handle exceptions that may occur during cache operations.
/// </summary>
/// <param name="cache"></param>
/// <param name="entityCacheOptions"></param>
/// <param name="serializer"></param>
/// <param name="logger"></param>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public class DefaultEntityCacheService<TKey, TEntity>(
    IDistributedCache cache,
    IEntityCacheOptions entityCacheOptions,
    IEntityCacheSerializer serializer,
    ILogger<DefaultEntityCacheService<TKey, TEntity>> logger)
    : IEntityCacheService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly Type _entityType = typeof(TEntity);
    private string CollectionKey => _entityType.Name;
    private string CollectionCacheKey => $"{CollectionKey}:All";

    private string GetCacheKey(string key)
    {
        var prefix = entityCacheOptions.CacheKeyPrefix();
        return string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
    }

    private string GetCacheKey(TKey? key)
    {
        if (key is null || key is 0 || (key is Guid guidKey && guidKey == default) ||
            string.IsNullOrEmpty(key.ToString()))
        {
            throw new ArgumentNullException(nameof(key), "Cache Key cannot be null or empty!");
        }

        return GetCacheKey($"{CollectionKey}:{key}");
    }

    public async Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);
        logger.LogDebug("Getting cache key {CacheKey}", cacheKey);

        try
        {
            var serialized = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrWhiteSpace(serialized))
            {
                logger.LogDebug("Cache key {CacheKey} not found", cacheKey);
                return null;
            }

            logger.LogDebug("Cache key {CacheKey} found", cacheKey);
            return serializer.Deserialize<TEntity>(serialized);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error getting cache key {CacheKey}", cacheKey);
            return null;
        }
    }

    public Task SetAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        SetAsync(entity, entityCacheOptions.CacheEntryOptions(_entityType), cancellationToken);

    public async Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(entity.Id);

        logger.LogDebug("Setting cache key {CacheKey}", cacheKey);

        var serialized = serializer.Serialize(entity);

        if (string.IsNullOrEmpty(serialized))
        {
            logger.LogWarning("Serialized entity is null or empty for key {CacheKey}", cacheKey);
            return;
        }

        try
        {
            await cache.SetStringAsync(cacheKey, serialized, cacheOptions, cancellationToken);
            logger.LogDebug("Cache key {CacheKey} set", cacheKey);
            await RemoveCollectionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error setting cache key {CacheKey}", cacheKey);
        }
    }

    public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);
        logger.LogDebug("Removing cache key {CacheKey}", cacheKey);
        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Cache key {CacheKey} removed", cacheKey);
            await RemoveCollectionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error removing cache key {CacheKey}", cacheKey);
        }
    }

    public async Task<List<TEntity>?> GetCollectionAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(CollectionCacheKey);
        logger.LogDebug("Getting collection cache key {CacheKey}", cacheKey);

        try
        {
            var serialized = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrWhiteSpace(serialized))
            {
                logger.LogDebug("Collection cache key {CacheKey} not found", cacheKey);
                return null;
            }

            logger.LogDebug("Collection cache key {CacheKey} found", cacheKey);

            return serializer.Deserialize<List<TEntity>>(serialized);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error getting collection cache key {CacheKey}", cacheKey);
            return null;
        }
    }

    public Task SetCollectionAsync(List<TEntity> entities, CancellationToken cancellationToken = default) =>
        SetCollectionAsync(entities, entityCacheOptions.CacheEntryOptions(_entityType), cancellationToken);

    public async Task SetCollectionAsync(List<TEntity> entities, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken = default)
    {
        if (entities.Count > entityCacheOptions.MaxCollectionSize)
        {
            logger.LogWarning("Collection size exceeds maximum allowed size of {MaxCollectionSize}",
                entityCacheOptions.MaxCollectionSize);
            return;
        }

        var cacheKey = GetCacheKey(CollectionCacheKey);
        logger.LogDebug("Setting collection cache key {CacheKey}", cacheKey);

        var serialized = serializer.Serialize(entities);

        if (string.IsNullOrEmpty(serialized))
        {
            logger.LogWarning("Serialized collection is null or empty for key {CacheKey}", cacheKey);
            return;
        }

        try
        {
            await cache.SetStringAsync(cacheKey, serialized, cacheOptions, cancellationToken);
            logger.LogDebug("Collection cache key {CacheKey} set", cacheKey);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error setting collection cache key {CacheKey}", cacheKey);
        }
    }

    public async Task RemoveCollectionAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(CollectionCacheKey);
        logger.LogDebug("Removing collection cache key {CacheKey}", cacheKey);
        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Collection cache key {CacheKey} removed", cacheKey);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error removing collection cache key {CacheKey}",
                cacheKey);
        }
    }
}
