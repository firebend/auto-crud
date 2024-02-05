using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency;

public class DistributedLockService : IDistributedLockService
{
    public ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
        => DistributedLockServiceStatics.Locker.LockAsync(key, cancellationToken);
}
