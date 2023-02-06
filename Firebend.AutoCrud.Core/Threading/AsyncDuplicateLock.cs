using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Threading
{
    public static class AsyncDuplicateLock
    {
        private static readonly AsyncKeyedLocker<string> KeyedLocker = new(o =>
        {
            o.PoolSize = 20;
            o.PoolInitialFill = 1;
        });

        public static IDisposable Lock(string key) => KeyedLocker.Lock(key);

        public static ValueTask<IDisposable> LockAsync(string key)
            => KeyedLocker.LockAsync(key);

        public static ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
            => KeyedLocker.LockAsync(key, cancellationToken);

        public static async ValueTask<AsyncKeyedLockTimeoutReleaser<string>> LockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default)
            => await KeyedLocker.LockAsync(key, timeout, cancellationToken).ConfigureAwait(false);
    }
}
