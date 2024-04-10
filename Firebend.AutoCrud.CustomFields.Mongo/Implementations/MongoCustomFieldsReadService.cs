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

namespace Firebend.AutoCrud.CustomFields.Mongo.Implementations;

public class MongoCustomFieldsReadService<TKey, TEntity> : BaseDisposable,
    ICustomFieldsReadService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    private readonly IMongoReadClient<TKey, TEntity> _readClient;
    private readonly ISessionTransactionManager _transactionManager;

    public MongoCustomFieldsReadService(IMongoReadClient<TKey, TEntity> readClient,
        ISessionTransactionManager transactionManager)
    {
        _readClient = readClient;
        _transactionManager = transactionManager;
    }


    ///********************************************
    // Author: JMA
    // Date: 2024-04-10
    // Comment: The custom fields of a mongo entity are stored in a nested array.
    // Therefore, we should only look up the entity by its id and do any kind of custom field filtering in memory.
    // This will allow us to use the id index when looking up the entity.
    //*******************************************
    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        entityTransaction ??= await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);

        if (entityTransaction is not null)
        {
            _transactionManager.AddTransaction(entityTransaction);
        }

        var result = await _readClient.GetFirstOrDefaultAsync(
            x => x.Id.Equals(entityId),
            entityTransaction,
            cancellationToken);

        if (result?.CustomFields is null)
        {
            return null;
        }

        return filter is null
            ? result.CustomFields
            : result.CustomFields.AsQueryable().Where(filter).ToList();
    }

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(entityId, filter, null, cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(
        TKey entityId,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(entityId, null, entityTransaction, cancellationToken);

    public Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId,
        TKey key,
        CancellationToken cancellationToken = default) =>
        GetByKeyAsync(entityId, key, null, cancellationToken);

    public Task<CustomFieldsEntity<TKey>> GetByKeyAsync(TKey entityId,
        TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default) =>
        FindFirstOrDefaultAsync(entityId, x => x.Id.Equals(key), transaction, cancellationToken);

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        CancellationToken cancellationToken = default) =>
        GetAllAsync(entityId, null, null, cancellationToken);

    public Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
        => ExistsAsync(entityId, filter, null, cancellationToken);

    public async Task<bool> ExistsAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default) =>
        (await GetAllAsync(entityId, filter, entityTransaction, cancellationToken))?.Count > 0;

    public Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default) =>
        FindFirstOrDefaultAsync(entityId, filter, null, cancellationToken);

    public async Task<CustomFieldsEntity<TKey>> FindFirstOrDefaultAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default) =>
        (await GetAllAsync(entityId, filter, entityTransaction, cancellationToken))?.FirstOrDefault();
}
