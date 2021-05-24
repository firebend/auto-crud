using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample
{
    public class CustomLockService : IDistributedLockService
    {
        private readonly ILogger _logger;
        private readonly DistributedLockService _locker;

        public CustomLockService(ILogger<CustomLockService> logger)
        {
            _logger = logger;
            _locker = new DistributedLockService();
        }

        public Task<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
        {
            _logger.LogDebug("I'm locking er up {Key}", key);
            return _locker.LockAsync(key, cancellationToken);
        }
    }
}
