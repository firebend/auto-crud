using System;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheOptions
{
    /// <summary>
    /// Function to get cache entry options
    /// </summary>
    public Func<Type, DistributedCacheEntryOptions> CacheEntryOptions { get; }

    /// <summary>
    /// Function to get cache key prefix. If null or empty, no prefix is used.
    /// </summary>
    public Func<string> CacheKeyPrefix { get; }

    /// <summary>
    /// The max collection size for the cache. If the collection size exceeds this value, the collection will not be cached.
    /// </summary>
    public int MaxCollectionSize { get; }
}
