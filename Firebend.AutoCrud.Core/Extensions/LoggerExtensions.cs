using System;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Core.Extensions;

public static class LoggerExtensions
{
    public static void LogDebug(this ILogger logger, Func<string> messageFunc)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(messageFunc());
        }
    }
}
