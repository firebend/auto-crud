using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntityReadService<TKey, TEntity>(
    IMongoReadClient<TKey, TEntity> readClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable, IEntityReadService<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    private readonly bool _usingCache = cacheService is not null;

    public async Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        if (_usingCache && transaction is null)
        {
            return await cacheService.GetOrSetAsync(key,
                () => readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), null, cancellationToken),
                cancellationToken);
        }
        return await readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), transaction, cancellationToken);
    }

    public Task<TEntity> GetByKeyAsync(TKey key, IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(transaction);
        return readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key), transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        if (_usingCache && transaction is null)
        {
            return await cacheService.GetOrSetAsync(
                () => readClient.GetAllAsync(null, null, cancellationToken),
                cancellationToken);
        }
        return await readClient.GetAllAsync(null, transaction, cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await readClient.GetAllAsync(filter, transaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        return readClient.GetAllAsync(null, entityTransaction, cancellationToken);
    }

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        return readClient.GetAllAsync(filter, entityTransaction, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await readClient.ExistsAsync(filter, transaction, cancellationToken);
    }

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(transaction);
        return readClient.ExistsAsync(filter, transaction, cancellationToken);
    }

    public async Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await readClient.GetFirstOrDefaultAsync(filter, transaction, cancellationToken);
    }

    public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        return readClient.GetFirstOrDefaultAsync(filter, entityTransaction, cancellationToken);
    }
}
