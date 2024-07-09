using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Client;

public class MongoRetryService : IMongoRetryService
{
    private static readonly TimeSpan TransactionTimeout = TimeSpan.FromSeconds(120);

    public async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries)
    {
        var tries = 0;
        double delay = 200;
        var now = Stopwatch.GetTimestamp();

        while (true)
        {
            try
            {
                return await method();
            }
            catch (Exception ex)
            {
                tries++;

                if (!ShouldRetry(ex, now) || tries >= maxTries)
                {
                    throw;
                }

                delay *= 2;

                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }
    }

    public static bool ShouldRetry(Exception exception, long now) =>
        exception switch
        {
            not null when exception.Message.Contains("duplicate key") => false,
            MongoWriteConcernException e => HasWriteConcern(e),
            MongoCommandException e => HasAnyErrorCode(e, ErrorCodes.Retryable),
            MongoBulkWriteException => false,
            MongoExecutionTimeoutException { Code: ErrorCodes.MaxTimeMsExpiredErrorCode } => true,
            MongoException e when HasAnyLabel(e,
                Labels.TransientTransaction,
                Labels.UnknownTransactionCommitResultLabel) => true,
            _ => HasTimedOut(now)
        };

    public static bool HasAnyErrorCode(MongoCommandException mongoException, params int[] codes)
        => codes.Contains(mongoException.Code);

    public static bool HasAnyLabel(MongoException mongoException, params string[] labels)
        => labels.Any(mongoException.HasErrorLabel);

    public static bool HasWriteConcern(MongoWriteConcernException writeConcernException)
    {
        var writeConcernError = writeConcernException
            .WriteConcernResult
            .Response?
            .GetValue("writeConcernError", null)
            ?.AsBsonDocument;

        var code = writeConcernError?.GetValue("code", -1).ToInt32() ?? 0;

        return code != 0 && ErrorCodes.Retryable.Contains(code);
    }

    private static bool HasTimedOut(long startTime, TimeSpan? timeout = null)
    {
        timeout ??= TransactionTimeout;
        var hasTimedOut = Stopwatch.GetElapsedTime(startTime) >= timeout;
        return hasTimedOut;
    }
}
