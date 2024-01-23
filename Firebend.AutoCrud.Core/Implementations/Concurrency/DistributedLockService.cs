using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;

internal static class DistributedLockServiceStatics
{
    public static readonly AsyncKeyedLocker<string> Locker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });
}

public class DistributedLockService : IDistributedLockService
{
    public ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
        => DistributedLockServiceStatics.Locker.LockAsync(key, cancellationToken);
}
