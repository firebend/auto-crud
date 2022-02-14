using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;

public static partial class AbstractDbCreatorLogger
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Creating database. DbName: {dbName}. DataSource: {dbSource}")]
    public static partial void CreatingDb(ILogger logger, string dbName, string dbSource);

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Database Created. DbName: {dbName}. DataSource: {dbSource}")]
    public static partial void DbCreated(ILogger logger, string dbName, string dbSource);
}
