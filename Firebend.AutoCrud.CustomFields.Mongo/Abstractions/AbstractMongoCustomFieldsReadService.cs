using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.CustomFields.Mongo.Abstractions;

public class AbstractMongoCustomFieldsReadService<TKey, TEntity> : BaseDisposable,
    ICustomFieldsReadService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly ISessionTransactionManager _transactionManager;

    private Expression<Func<TEntity, bool>> _filterByEntityId(TKey entityId) =>
        entity => entity.Id.Equals(entityId);

    protected AbstractMongoCustomFieldsReadService(IMongoReadClient<TKey, TEntity> readClient,
        ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _transactionManager = transactionManager;
    }

    public async Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId, TKey key,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await GetByKeyAsync(entityId, key, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId, TKey key, IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);

        var result = await _readClient.GetFirstOrDefaultAsync(
            x => x.Id.Equals(entityId) && x.CustomFields.Any(cf => cf.Id.Equals(key)), transaction, cancellationToken);
        return result.CustomFields.FirstOrDefault(cf => cf.Id.Equals(key));
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await GetAllAsync(entityId, null, transaction, cancellationToken);
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await GetAllAsync(entityId, filter, transaction, cancellationToken);
    }

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return GetAllAsync(entityId, null, entityTransaction, cancellationToken);
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var result =
            await _readClient.GetFirstOrDefaultAsync(_filterByEntityId(entityId), entityTransaction, cancellationToken);
        if (result?.CustomFields is null)
        {
            return null;
        }

        return filter == null
            ? result.CustomFields
            : result.CustomFields.AsQueryable().Where(filter).ToList();
    }

    public async Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await ExistsAsync(entityId, filter, transaction, cancellationToken);
    }

    public async Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var result = await _readClient.GetFirstOrDefaultAsync(_filterByEntityId(entityId), entityTransaction, cancellationToken);
        if (result?.CustomFields is null)
        {
            return false;
        }
        return result.CustomFields.AsQueryable().Any(filter);
    }

    public async Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await FindFirstOrDefaultAsync(entityId, filter, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var result =
            await _readClient.GetFirstOrDefaultAsync(_filterByEntityId(entityId), entityTransaction, cancellationToken);
        if (result?.CustomFields is null)
        {
            return null;
        }

        return result.CustomFields.AsQueryable().Where(filter).FirstOrDefault();
    }
}
