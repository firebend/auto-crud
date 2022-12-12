using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;


internal static class MemoizeCaches
{
    public static readonly ConcurrentDictionary<string, object> Caches = new();
}

public class Memoizer : IMemoizer
{
    public static Memoizer Instance { get; } = new();

    public async Task<T> MemoizeAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
    {
        if (TryGetFromCache<T>(key, out var returnValue))
        {
            return returnValue;
        }

        var value = await factory();
        await AddToCache(key, value, cancellationToken);
        return value;
    }

    public async Task<T> MemoizeAsync<T, TArg>(string key, Func<TArg, Task<T>> factory, TArg arg, CancellationToken cancellationToken)
    {
        if (TryGetFromCache<T>(key, out var returnValue))
        {
            return returnValue;
        }

        var value = await factory(arg);
        await AddToCache(key, value, cancellationToken);
        return value;
    }

    public T Memoize<T>(string key, Func<T> factory)
    {
        if (TryGetFromCache<T>(key, out var returnValue))
        {
            return returnValue;
        }

        var value = factory();
        AddToCache(key, value, default).GetAwaiter().GetResult();
        return value;
    }


    public T Memoize<T, TArg>(string key, Func<TArg, T> factory, TArg arg)
    {
        if (TryGetFromCache<T>(key, out var returnValue))
        {
            return returnValue;
        }

        var value = factory(arg);
        AddToCache(key, value, default).GetAwaiter().GetResult();
        return value;
    }

    private static bool TryGetFromCache<T>(string key, out T memoizeAsync)
    {
        if (MemoizeCaches.Caches.TryGetValue(key, out var cached))
        {
            memoizeAsync = (T)cached;
            return true;
        }

        memoizeAsync = default;
        return false;
    }

    private static async Task AddToCache<T>(string key, T value, CancellationToken cancellationToken)
    {
        var count = 0;

        while (count < 10)
        {
            try
            {
                if (MemoizeCaches.Caches.TryAdd(key, value))
                {
                    return;
                }

                await Task.Delay(100, cancellationToken);
            }
            catch (OverflowException)
            {
                MemoizeCaches.Caches.Clear();
            }
            finally
            {
                count++;
            }
        }
    }
}
