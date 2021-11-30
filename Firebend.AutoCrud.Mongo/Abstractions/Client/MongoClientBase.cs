using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public abstract class MongoClientBase : BaseDisposable
    {
        protected MongoClientBase(IMongoClient client,
            ILogger logger,
            IMongoRetryService mongoRetryService)
        {
            Client = client;
            Logger = logger;
            MongoRetryService = mongoRetryService;
        }

        protected IMongoClient Client { get; }

        protected ILogger Logger { get; }

        protected IMongoRetryService MongoRetryService { get; }

        protected Task RetryErrorAsync(Func<Task> method) => RetryErrorAsync(async () =>
        {
            await method();
            return true;
        });

        protected async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries = 7)
        {
            try
            {
                return await MongoRetryService.RetryErrorAsync(method, maxTries);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error querying Document Store: {Message}", ex.Message);
                throw;
            }
        }
    }
}
