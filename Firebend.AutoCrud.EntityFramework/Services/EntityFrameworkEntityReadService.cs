using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Services;

public class EntityFrameworkEntityReadService<TKey, TEntity> : BaseDisposable,
    IEntityReadService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IEntityFrameworkQueryClient<TKey, TEntity> _readClient;
    private readonly ISessionTransactionManager _transactionManager;

    public EntityFrameworkEntityReadService(IEntityFrameworkQueryClient<TKey, TEntity> readClient,
        ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), true, transaction, cancellationToken);
    }

    public Task<TEntity> GetByKeyAsync(TKey key, IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);
        return _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), true, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetAllAsync(null, true, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _readClient.GetAllAsync(filter, true, transaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetAllAsync(null, true, entityTransaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetAllAsync(filter, true, entityTransaction, cancellationToken);
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
        return await FindFirstOrDefaultAsync(filter, transaction, cancellationToken);
    }

    public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _readClient.GetFirstOrDefaultAsync(filter, true, entityTransaction, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _readClient?.Dispose();
}
