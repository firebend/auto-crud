using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

public class DefaultEntityCacheService<TKey, TEntity>(
    IDistributedCache cache,
    IEntityCacheOptions entityCacheOptions,
    ILogger<DefaultEntityCacheService<TKey, TEntity>> logger) : IEntityCacheService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private static string CollectionKey => typeof(TEntity).Name;
    private static string CollectionCacheKey => $"{CollectionKey}:All";

    private string GetCacheKey(TKey key)
    {
        if (string.IsNullOrEmpty(key.ToString()))
        {
            throw new ArgumentNullException(nameof(key), "Cache Key cannot be null or empty!");
        }

        return $"{CollectionKey}:{key}";
    }

    public async Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(key);

        try
        {
            var serialized = await cache.GetStringAsync(cacheKey, cancellationToken);

            return string.IsNullOrWhiteSpace(serialized)
                ? null
                : entityCacheOptions.Serializer.Deserialize<TEntity>(serialized);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error getting cache key {CacheKey}", cacheKey);
            return null;
        }
    }

    public Task SetAsync(TEntity entity, CancellationToken cancellationToken) =>
        SetAsync(entity, entityCacheOptions.GetCacheEntryOptions(entity), cancellationToken);

    public async Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(entity.Id);
        var serialized = entityCacheOptions.Serializer.Serialize(entity);

        try
        {
            await cache.SetStringAsync(cacheKey, serialized, cacheOptions, cancellationToken);
            await RemoveCollectionAsync(cancellationToken);
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
            await RemoveCollectionAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error removing cache key {CacheKey}", cacheKey);
        }
    }

    public async Task<List<TEntity>> GetCollectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var serialized = await cache.GetStringAsync(CollectionCacheKey, cancellationToken);

            return string.IsNullOrWhiteSpace(serialized)
                ? null
                : entityCacheOptions.Serializer.Deserialize<List<TEntity>>(serialized);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error getting collection cache key {CacheKey}", CollectionCacheKey);
            return null;
        }
    }

    public Task SetCollectionAsync(List<TEntity> entities, CancellationToken cancellationToken) =>
        SetCollectionAsync(entities, entityCacheOptions.GetCacheEntryOptions(entities), cancellationToken);

    public async Task SetCollectionAsync(List<TEntity> entities, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken)
    {
        if (entities.Count > entityCacheOptions.MaxCollectionSize)
        {
            logger.LogWarning("Collection size exceeds maximum allowed size of {MaxCollectionSize}",
                entityCacheOptions.MaxCollectionSize);
            return;
        }

        var serialized = entityCacheOptions.Serializer.Serialize(entities);
        try
        {
            await cache.SetStringAsync(CollectionCacheKey, serialized, cacheOptions, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error setting collection cache key {CacheKey}", CollectionCacheKey);
        }
    }

    public async Task RemoveCollectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(CollectionCacheKey, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "Error removing collection cache key {CacheKey}",
                CollectionCacheKey);
        }
    }
}
