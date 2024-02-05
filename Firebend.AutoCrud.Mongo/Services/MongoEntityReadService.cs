using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntityReadService<TKey, TEntity> : BaseDisposable, IEntityReadService<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly ISessionTransactionManager _transactionManager;

    public MongoEntityReadService(IMongoReadClient<TKey, TEntity> readClient,
        ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), transaction, cancellationToken);
    }

    public Task<TEntity> GetByKeyAsync(TKey key, IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);
        return _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetAllAsync(null, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetAllAsync(filter, transaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetAllAsync(null, entityTransaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetAllAsync(filter, entityTransaction, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.ExistsAsync(filter, transaction, cancellationToken);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);
        return _readClient.ExistsAsync(filter, transaction, cancellationToken);
    }

    public async Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetFirstOrDefaultAsync(filter, transaction, cancellationToken);
    }

    public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetFirstOrDefaultAsync(filter, entityTransaction, cancellationToken);
    }
}
