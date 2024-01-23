using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample;

public partial class CustomLockService : IDistributedLockService
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "I'm locking 'er up {key}")]
    public static partial void LogMessage(ILogger logger, string key);

    private readonly ILogger _logger;
    private readonly DistributedLockService _locker;

    public CustomLockService(ILogger<CustomLockService> logger)
    {
        _logger = logger;
        _locker = new DistributedLockService();
    }

    public ValueTask<IDisposable> LockAsync(string key, CancellationToken cancellationToken)
    {
        LogMessage(_logger, key);
        return _locker.LockAsync(key, cancellationToken);
    }
}
