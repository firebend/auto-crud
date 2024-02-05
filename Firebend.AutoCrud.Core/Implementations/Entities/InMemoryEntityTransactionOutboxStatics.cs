using AsyncKeyedLock;

namespace Firebend.AutoCrud.Core.Implementations.Entities;

internal static class InMemoryEntityTransactionOutboxStatics
{
    public static readonly AsyncKeyedLocker<string> Locker = new(o =>
    {
        o.PoolSize = 20;
        o.PoolInitialFill = 1;
    });
}
