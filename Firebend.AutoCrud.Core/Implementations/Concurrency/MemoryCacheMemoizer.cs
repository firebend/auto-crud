using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Microsoft.Extensions.Caching.Memory;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;


public class MemoryCacheMemoizer : IMemoizer
{
    public static MemoryCacheMemoizer Instance { get; } = new();

    private static MemoryCache GetCache() => new(new MemoryCacheOptions());

    private static readonly AsyncKeyedLocker<string> Locker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });

    public async Task<T> MemoizeAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
    {
        using var _ = await Locker.LockAsync(key, cancellationToken);

        using var cache = GetCache();

        if (TryGetFromCache<T>(cache, key, out var returnValue))
        {
            return returnValue;
        }

        var value = await factory();
        AddToCache(cache ,key, value);
        return value;
    }

    public async Task<T> MemoizeAsync<T, TArg>(string key, Func<TArg, Task<T>> factory, TArg arg, CancellationToken cancellationToken)
    {
        using var _ = await Locker.LockAsync(key, cancellationToken);
        using var cache = GetCache();

        if (TryGetFromCache<T>(cache, key, out var returnValue))
        {
            return returnValue;
        }

        var value = await factory(arg);
        AddToCache(cache, key, value);
        return value;
    }

    public T Memoize<T>(string key, Func<T> factory)
    {
        using var _ = Locker.Lock(key);
        using var cache = GetCache();

        if (TryGetFromCache<T>(cache, key, out var returnValue))
        {
            return returnValue;
        }

        var value = factory();
        AddToCache(cache, key, value);
        return value;
    }


    public T Memoize<T, TArg>(string key, Func<TArg, T> factory, TArg arg)
    {
        using var _ = Locker.Lock(key);
        using var cache = GetCache();
        if (TryGetFromCache<T>(cache, key, out var returnValue))
        {
            return returnValue;
        }

        var value = factory(arg);
        AddToCache(cache, key, value);
        return value;
    }

    private static bool TryGetFromCache<T>(MemoryCache cache, string key, out T memoizeAsync)
    {
        if (cache.TryGetValue(key, out var cached))
        {
            memoizeAsync = (T)cached;
            return true;
        }

        memoizeAsync = default;
        return false;
    }

    private static void AddToCache<T>(MemoryCache cache, string key, T value)
    {
        using var entry = cache.CreateEntry(key);
        entry.Value = value;
        entry.SlidingExpiration = TimeSpan.FromSeconds(30);
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
    }
}
