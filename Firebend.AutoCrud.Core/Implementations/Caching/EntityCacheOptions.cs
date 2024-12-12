#nullable enable
using System;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

public class EntityCacheOptions : IEntityCacheOptions
{
    /// <summary>
    /// Function to get cache entry options. Defaults to 1 hour
    /// </summary>
    public Func<Type, DistributedCacheEntryOptions> CacheEntryOptions { get; set; } = _ =>
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

    /// <summary>
    /// Function to get cache key prefix. Defaults to empty string.
    /// If null or empty, no prefix is used.
    /// </summary>
    public Func<string> CacheKeyPrefix { get; set; } = () => string.Empty;

    /// <summary>
    /// The max collection size for the cache. If the collection size exceeds this value, the collection will not be cached.
    /// Defaults to 200
    /// </summary>
    public int MaxCollectionSize { get; set; } = 200;
}
