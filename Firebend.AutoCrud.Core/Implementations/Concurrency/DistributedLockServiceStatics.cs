using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;

internal static class DistributedLockServiceStatics
{
    public static readonly AsyncKeyedLocker<string> Locker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });
}
