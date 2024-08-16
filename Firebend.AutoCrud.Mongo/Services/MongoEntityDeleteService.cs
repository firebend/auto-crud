using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntityDeleteService<TKey, TEntity> : BaseDisposable, IEntityDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IMongoDeleteClient<TKey, TEntity> _deleteClient;
    private readonly ISessionTransactionManager _transactionManager;

    public MongoEntityDeleteService(IMongoDeleteClient<TKey, TEntity> deleteClient,
        ISessionTransactionManager transactionManager)
    {
        _deleteClient = deleteClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _deleteClient.DeleteAsync(x => x.Id.Equals(key), transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        _transactionManager.AddTransaction(entityTransaction);
        return _deleteClient.DeleteAsync(x => x.Id.Equals(key), entityTransaction, cancellationToken);
    }
}
