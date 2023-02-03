using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Concurrency
{
    public interface IDistributedLockService
    {
        ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken);
    }
}
