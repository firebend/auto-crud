using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Threading;

namespace Firebend.AutoCrud.Core.Implementations.Concurrency
{
    public class DistributedLockService : IDistributedLockService
    {
        public async Task<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
        {
            var lockObj = await AsyncDuplicateLock.LockAsync(key, cancellationToken);
            return lockObj;
        }
    }
}
