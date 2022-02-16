using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration;

public static partial class BaseMongoConfigureCollectionLogger
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Configuring collection for {collection}")]
    public static partial void ConfiguringCollection(ILogger logger, string collection);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Configuring indexes for {collection}")]
    public static partial void ConfiguringIndexes(ILogger logger, string collection);
}
