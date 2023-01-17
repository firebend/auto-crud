using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Threading
{
    public static class AsyncDuplicateLock
    {
        private static readonly AsyncKeyedLock KeyedLocker = new();

        public static IDisposable Lock(object key) => KeyedLocker.Lock(key);

        public static ValueTask<IDisposable> LockAsync(object key)
        {
            return KeyedLocker.LockAsync(key);
        }

        public static ValueTask<IDisposable> LockAsync(object key, CancellationToken cancellationToken)
        {
            return _asyncKeyedLocker.LockAsync(key, cancellationToken);
        }

        public static async ValueTask<IDisposable> LockAsync(object key, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                return await KeyedLocker.LockAsync(key, timeout.Value, cancellationToken).ConfigureAwait(false);
            }
            return await KeyedLocker.LockAsync(key, cancellationToken).ConfigureAwait(false);
        }
    }
}
