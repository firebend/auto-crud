using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public abstract class MongoClientBase<TKey, TEntity> : BaseDisposable
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IMongoClientFactory<TKey, TEntity> _mongoClientFactory;
        private IMongoClient _mongoClient;

        protected MongoClientBase(IMongoClientFactory<TKey, TEntity> clientFactory,
            ILogger logger,
            IMongoRetryService mongoRetryService)
        {
            _mongoClientFactory = clientFactory;
            Logger = logger;
            MongoRetryService = mongoRetryService;
        }

        protected async Task<IMongoClient> GetClientAsync(string overrideShardKey = null)
        {
            if (_mongoClient != null)
            {
                return _mongoClient;
            }

            _mongoClient = await _mongoClientFactory.CreateClientAsync(overrideShardKey);
            return _mongoClient;
        }

        protected ILogger Logger { get; }

        protected IMongoRetryService MongoRetryService { get; }

        protected virtual Task RetryErrorAsync(Func<Task> method) => RetryErrorAsync(async () =>
        {
            await method();
            return true;
        });

        protected virtual async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries = 7)
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
