using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Client;

public abstract class MongoClientBase<TKey, TEntity> : BaseDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IMongoClientFactory<TKey, TEntity> _mongoClientFactory;
    private IMongoClient _mongoClient;

    public LinqProvider? LinqProvider => _mongoClient?.Settings?.LinqProvider;

    protected MongoClientBase(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger logger,
        IMongoRetryService mongoRetryService)
    {
        _mongoClientFactory = clientFactory;
        Logger = logger;
        MongoRetryService = mongoRetryService;
    }

    protected async Task<IMongoClient> GetClientAsync(string overrideShardKey, CancellationToken cancellationToken)
    {
        if (_mongoClient != null)
        {
            return _mongoClient;
        }

        _mongoClient = await _mongoClientFactory.CreateClientAsync(overrideShardKey, cancellationToken);
        return _mongoClient;
    }

    protected ILogger Logger { get; }

    protected IMongoRetryService MongoRetryService { get; }

    protected virtual Task RetryErrorAsync(Func<Task> method) => RetryErrorAsync(async () =>
    {
        await method();
        return true;
    });

    protected virtual async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int? maxTries = null)
    {
        try
        {
            return await MongoRetryService.RetryErrorAsync(method, maxTries.GetValueOrDefault(MongoClientBaseDefaults.NumberOfRetries));
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error querying Document Store: {Message}", ex.Message);
            throw;
        }
    }
}
