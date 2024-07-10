using System;
using System.Diagnostics;
using System.Linq;
using MongoDB.Driver;
// ReSharper disable MemberCanBePrivate.Global

namespace Firebend.AutoCrud.Mongo.Client;

public static class MongoRetryUtilities
{
    private static readonly TimeSpan TransactionTimeout = TimeSpan.FromSeconds(120);

    public static bool ShouldRetry(Exception exception, long now) =>
        exception switch
        {
            not null when exception.Message.Contains("duplicate key") => false,
            MongoWriteConcernException e => e.HasWriteConcern(),
            MongoCommandException e => e.HasAnyErrorCode(ErrorCodes.Retryable),
            MongoBulkWriteException => false,
            MongoExecutionTimeoutException { Code: ErrorCodes.MaxTimeMsExpiredErrorCode } => true,
            MongoException e when e.HasAnyLabel(Labels.TransientTransaction, Labels.UnknownTransactionCommitResultLabel) => true,
            _ => HasTimedOut(now)
        };

    public static bool HasAnyErrorCode(this MongoCommandException mongoException, params int[] codes)
        => codes.Contains(mongoException.Code);

    public static bool HasAnyLabel(this MongoException mongoException, params string[] labels)
        => labels.Any(mongoException.HasErrorLabel);

    public static bool HasWriteConcern(this MongoWriteConcernException writeConcernException)
    {
        var writeConcernError = writeConcernException
            .WriteConcernResult
            .Response?
            .GetValue("writeConcernError", null)
            ?.AsBsonDocument;

        var code = writeConcernError?.GetValue("code", -1).ToInt32() ?? 0;

        return code != 0 && ErrorCodes.Retryable.Contains(code);
    }

    public static bool HasTimedOut(long startTime, TimeSpan? timeout = null)
    {
        timeout ??= TransactionTimeout;
        var hasTimedOut = Stopwatch.GetElapsedTime(startTime) >= timeout;
        return hasTimedOut;
    }
}
