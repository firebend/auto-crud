using System;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client;

public static partial class MongoClientFactoryLogger
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "MONGO: {commandName} - {command}")]
    public static partial void Started(ILogger logger, string commandName, BsonDocument command);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message ="SUCCESS: {commandName}({duration}) - {reply}")]
    public static partial void Success(ILogger logger, string commandName, TimeSpan duration, BsonDocument reply);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "ERROR: {commandName}({duration})")]
    public static partial void Failed(ILogger logger, string commandName, TimeSpan duration);
}
