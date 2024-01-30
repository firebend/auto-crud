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
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Implementations;

public class EfCustomFieldsReadService<TKey, TEntity, TCustomFieldsEntity> : BaseDisposable,
    ICustomFieldsReadService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>, new()
{
    private readonly IEntityFrameworkQueryClient<Guid, TCustomFieldsEntity> _readClient;
    private readonly ISessionTransactionManager _transactionManager;

    private static Expression<Func<TCustomFieldsEntity, bool>> FilterByEntityId(TKey entityId) =>
        entity => entity.EntityId.Equals(entityId);

    public EfCustomFieldsReadService(IEntityFrameworkQueryClient<Guid, TCustomFieldsEntity> readClient,
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
        var result =
            await _readClient.GetFirstOrDefaultAsync(x => x.Id.Equals(key) && x.EntityId.Equals(entityId), true,
                transaction, cancellationToken);
        return result?.ToCustomFields();
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await GetAllAsync(entityId, null, transaction, cancellationToken);
    }

    public Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return GetAllAsync(entityId, null, entityTransaction, cancellationToken);
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await GetAllAsync(entityId, filter, transaction, cancellationToken);
    }

    public async Task<List<CustomFieldsEntity<TKey>>> GetAllAsync(TKey entityId,
        Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        var result =
            await _readClient.GetAllAsync(FilterByEntityId(entityId), true, entityTransaction, cancellationToken);
        var asCustomFields = result.Select(x => x?.ToCustomFields());
        return filter == null ? asCustomFields.ToList() : asCustomFields.AsQueryable().Where(filter).ToList();
    }

    public async Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await ExistsAsync(entityId, filter, transaction, cancellationToken);
    }

    public async Task<bool> ExistsAsync(TKey entityId, Expression<Func<CustomFieldsEntity<TKey>, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(transaction);
        var result = await _readClient.GetAllAsync(FilterByEntityId(entityId), true, transaction, cancellationToken);
        return result.Select(x => x?.ToCustomFields()).AsQueryable().Any(filter);
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
            await _readClient.GetAllAsync(FilterByEntityId(entityId), true, entityTransaction, cancellationToken);
        return result.Select(x => x?.ToCustomFields()).AsQueryable().FirstOrDefault(filter);
    }

    protected override void DisposeManagedObjects() => _readClient?.Dispose();
}
