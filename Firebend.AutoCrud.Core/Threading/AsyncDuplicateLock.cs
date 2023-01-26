using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Threading
{
    public static class AsyncDuplicateLock
    {
        private static readonly AsyncKeyedLocker KeyedLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        public static IDisposable Lock(object key) => KeyedLocker.Lock(key);

        public static ValueTask<IDisposable> LockAsync(object key)
        {
            return KeyedLocker.LockAsync(key);
        }

        public static ValueTask<IDisposable> LockAsync(object key, CancellationToken cancellationToken)
        {
            return KeyedLocker.LockAsync(key, cancellationToken);
        }

        public static async ValueTask<AsyncKeyedLockTimeoutReleaser<object>> LockAsync(object key, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            return await KeyedLocker.LockAsync(key, timeout, cancellationToken).ConfigureAwait(false);
        }
    }
}
