using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Threading;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;

public class Memoizer<T> : IMemoizer<T>
{
    private static readonly ConcurrentDictionary<string, T> Caches = new();

    public async Task<T> MemoizeAsync(string key, Func<Task<T>> factory, CancellationToken cancellationToken)
    {
        if (Caches.TryGetValue(key, out var cached))
        {
            return cached;
        }

        using var dupLock = AsyncDuplicateLock.LockAsync(key, cancellationToken);

        if (Caches.TryGetValue(key, out cached))
        {
            return cached;
        }

        var value = await factory();

        var count = 0;

        while (count < 10)
        {
            try
            {
                if (Caches.TryAdd(key, value))
                {
                    return value;
                }

                await Task.Delay(100, cancellationToken);
            }
            catch (OverflowException)
            {
                Caches.Clear();
            }
            finally
            {
                count++;
            }
        }

        return value;
    }

    public T Memoize(string key, Func<T> factory)
    {
        if (Caches.TryGetValue(key, out var cached))
        {
            return cached;
        }

        using var dupLock = AsyncDuplicateLock.Lock(key);

        if (Caches.TryGetValue(key, out cached))
        {
            return cached;
        }

        var value = factory();

        var count = 0;

        while (count < 10)
        {
            try
            {
                if (Caches.TryAdd(key, value))
                {
                    return value;
                }

                Task.Delay(100).GetAwaiter().GetResult();
            }
            catch (OverflowException)
            {
                Caches.Clear();
            }
            finally
            {
                count++;
            }
        }

        return value;
    }
}
