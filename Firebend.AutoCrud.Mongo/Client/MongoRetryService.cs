using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Client;

public class MongoRetryService : IMongoRetryService
{
    public async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries)
    {
        var tries = 0;
        var delay = 200;
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

                if (MongoRetryUtilities.ShouldRetry(ex, now) is false)
                {
                    throw;
                }

                if (maxTries > 0 && tries >= maxTries)
                {
                    throw;
                }

                delay *= 2;

                await Task.Delay(delay);
            }
        }
    }
}
