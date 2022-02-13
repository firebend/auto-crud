using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.HostedServices;

public static partial class ConfigureCollectionsHostedServiceLogger
{

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Configuring all mongo collections")]
    public static partial void Start(ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Finished configuring all mongo collections")]
    public static partial void Finish(ILogger logger);
}
