using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public abstract class MongoClientBase
    {
        protected MongoClientBase(IMongoClient client,
            ILogger logger)
        {
            Client = client;
            Logger = logger;
        }

        protected IMongoClient Client { get; }

        protected ILogger Logger { get; }

        //todo: we may want to refactor this with polly or some other retry interface abstraction 
        protected Task RetryErrorAsync(Func<Task> method)
        {
            return RetryErrorAsync(async () =>
            {
                await method();
                return true;
            });
        }

        protected async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, bool retry = true)
        {
            try
            {
                return await method();
            }
            catch (Exception ex)
            {
                if (retry)
                    return await RetryErrorAsync(method, false);

                Logger?.LogError(ex, "Error querying Document Store: \"{Message}\"", ex.Message);

                throw;
            }
        }
    }
}
