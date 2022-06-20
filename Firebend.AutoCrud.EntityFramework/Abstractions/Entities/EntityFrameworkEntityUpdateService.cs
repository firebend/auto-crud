using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities;

public abstract class EntityFrameworkEntityUpdateService<TKey, TEntity> : BaseDisposable,
    IEntityUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IEntityFrameworkUpdateClient<TKey, TEntity> _updateClient;
    private readonly ISessionTransactionManager _transactionManager;

    protected EntityFrameworkEntityUpdateService(IEntityFrameworkUpdateClient<TKey, TEntity> updateClient,
        ISessionTransactionManager transactionManager)
    {
        _updateClient = updateClient;
        _transactionManager = transactionManager;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _updateClient.UpdateAsync(entity, transaction, cancellationToken);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _updateClient.UpdateAsync(entity, entityTransaction, cancellationToken);
    }

    public virtual async Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _updateClient.UpdateAsync(key, jsonPatchDocument, transaction, cancellationToken);
    }

    public Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument,
        IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _updateClient.UpdateAsync(key, jsonPatchDocument, entityTransaction, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _updateClient?.Dispose();
}
