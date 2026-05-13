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
    ITenantEntityCacheKeyResolver tenantEntityCacheKeyResolver,
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

    private async Task<string> GetCacheKey(TKey? key, CancellationToken cancellationToken)
    {
        if (key is null || EqualityComparer<TKey>.Default.Equals(key.Value, default) ||
            string.IsNullOrEmpty(key.ToString()))
        {
            throw new ArgumentNullException(nameof(key), "Cache Key cannot be null or empty!");
        }

        var tenantIdSegment = await GetTenantIdSegment(cancellationToken);
        var cacheKey = tenantIdSegment is null
            ? $"{CollectionKey}:{key}"
            : $"{tenantIdSegment}:{CollectionKey}:{key}";

        return GetCacheKey(cacheKey);
    }

    private async Task<string> GetCollectionCacheKey(CancellationToken cancellationToken)
    {
        var tenantIdSegment = await GetTenantIdSegment(cancellationToken);
        var cacheKey = tenantIdSegment is null
            ? CollectionCacheKey
            : $"{tenantIdSegment}:{CollectionCacheKey}";

        return GetCacheKey(cacheKey);
    }

    private async Task<string?> GetTenantIdSegment(CancellationToken cancellationToken)
        => await tenantEntityCacheKeyResolver.GetTenantIdSegmentAsync(_entityType, cancellationToken);

    public async Task<TEntity?> GetAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var cacheKey = await GetCacheKey(key, cancellationToken);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error getting entity cache for id {EntityId} and cache key {CacheKey}", key,
                cacheKey);
            return null;
        }
    }

    public Task SetAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        SetAsync(entity, entityCacheOptions.CacheEntryOptions(_entityType), cancellationToken);

    public async Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = await GetCacheKey(entity.Id, cancellationToken);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error setting entity cache for id {EntityId} and cache key {CacheKey}", entity.Id,
                cacheKey);
        }
    }

    public async Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var cacheKey = await GetCacheKey(key, cancellationToken);
        logger.LogDebug("Removing cache key {CacheKey}", cacheKey);

        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Cache key {CacheKey} removed", cacheKey);
            await RemoveCollectionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error removing entity cache for id {EntityId} and cache key {CacheKey}", key,
                cacheKey);
        }
    }

    public async Task<List<TEntity>?> GetCollectionAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = await GetCollectionCacheKey(cancellationToken);
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
        catch (OperationCanceledException)
        {
            throw;
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

        var cacheKey = await GetCollectionCacheKey(cancellationToken);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error setting collection cache key {CacheKey}", cacheKey);
        }
    }

    public async Task RemoveCollectionAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = await GetCollectionCacheKey(cancellationToken);
        logger.LogDebug("Removing collection cache key {CacheKey}", cacheKey);

        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Collection cache key {CacheKey} removed", cacheKey);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error removing collection cache key {CacheKey}",
                cacheKey);
        }
    }
}
