using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Client;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class MongoEntityTransactionFactory<TKey, TEntity> :
    MongoClientBase<TKey, TEntity>, IEntityTransactionFactory<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IEntityTransactionOutbox _outbox;
    private readonly IMongoConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly ILoggerFactory _loggerFactory;

    public MongoEntityTransactionFactory(IMongoClientFactory<TKey, TEntity> factory,
        ILoggerFactory loggerFactory,
        IEntityTransactionOutbox outbox,
        IMongoRetryService retryService,
        IMongoConnectionStringProvider<TKey, TEntity> connectionStringProvider) :
        base(factory, loggerFactory.CreateLogger<MongoEntityTransactionFactory<TKey, TEntity>>(), retryService)
    {
        _outbox = outbox;
        _connectionStringProvider = connectionStringProvider;
        _loggerFactory = loggerFactory;
    }

    public async Task<string> GetDbContextHashCode()
    {
        var connectionString = await _connectionStringProvider.GetConnectionStringAsync();
        var hashCode = connectionString.GetHashCode();
        return $"mongo_{hashCode}";
    }

    public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
    {
        var client = await GetClientAsync();
        var session = await client.StartSessionAsync(MongoEntityTransactionFactoryDefaults.SessionOptions, cancellationToken);
        session.StartTransaction(MongoEntityTransactionFactoryDefaults.TransactionOptions);
        return new MongoEntityTransaction(session, _outbox, MongoRetryService, _loggerFactory);
    }

    public bool ValidateTransaction(IEntityTransaction transaction)
        => transaction is MongoEntityTransaction mongoTransaction && mongoTransaction.ClientSessionHandle.IsInTransaction;
}
