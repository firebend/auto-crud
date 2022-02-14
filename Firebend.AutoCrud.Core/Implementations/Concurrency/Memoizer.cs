using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Threading;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;

internal static class MemoizeCaches<T>
{
    public static readonly ConcurrentDictionary<string, T> Caches = new();
}

public class Memoizer<T> : IMemoizer<T>
{
    public async Task<T> MemoizeAsync(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
    {
        if (TryGetFromCache(key, out var returnValue))
        {
            return returnValue;
        }

        using var dupLock = await AsyncDuplicateLock.LockAsync(key, cancellationToken);

        if (TryGetFromCache(key, out var returnValueAgain))
        {
            return returnValueAgain;
        }

        var value = await factory();

        try
        {
            await AddToCache(key, value, cancellationToken);
        }
        catch
        {
            // ignore
        }

        return value;
    }

    public async Task<T> MemoizeAsync<TArg>(string key, Func<TArg, Task<T>> factory, TArg arg, CancellationToken cancellationToken)
    {
        if (TryGetFromCache(key, out var returnValue))
        {
            return returnValue;
        }

        using var dupLock = await AsyncDuplicateLock.LockAsync(key, cancellationToken);

        if (TryGetFromCache(key, out var returnValueAgain))
        {
            return returnValueAgain;
        }

        var value = await factory(arg);

        try
        {
            await AddToCache(key, value, cancellationToken);
        }
        catch
        {
            // ignore
        }

        return value;

    }

    private static bool TryGetFromCache(string key, out T memoizeAsync)
    {
        if (MemoizeCaches<T>.Caches.TryGetValue(key, out var cached))
        {
            memoizeAsync = cached;
            return true;
        }

        memoizeAsync = default;
        return false;
    }

    private static async Task AddToCache(string key, T value, CancellationToken cancellationToken)
    {
        var count = 0;

        while (count < 10)
        {
            try
            {
                if (MemoizeCaches<T>.Caches.TryAdd(key, value))
                {
                    return;
                }

                await Task.Delay(100, cancellationToken);
            }
            catch (OverflowException)
            {
                MemoizeCaches<T>.Caches.Clear();
            }
            finally
            {
                count++;
            }
        }
    }
}
