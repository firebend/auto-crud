using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Client.Configuration;

public static partial class BaseMongoConfigureCollectionLogger
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Configuring collection for {db} {collection}")]
    public static partial void ConfiguringCollection(ILogger logger, string db, string collection);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Configuring indexes for {db} {collection}")]
    public static partial void ConfiguringIndexes(ILogger logger, string db, string collection);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Done configuring {db} {collection}")]
    public static partial void Done(ILogger logger, string db, string collection);
}
