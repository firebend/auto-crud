using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Services;

public class EntityFrameworkEntityUpdateService<TKey, TEntity>(
    IEntityFrameworkUpdateClient<TKey, TEntity> updateClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable,
        IEntityUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        var updated = await updateClient.UpdateAsync(entity, transaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        var updated = await updateClient.UpdateAsync(entity, entityTransaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public virtual async Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        var updated = await updateClient.UpdateAsync(key, jsonPatchDocument, transaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public async Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument,
        IEntityTransaction entityTransaction, CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        var updated = await updateClient.UpdateAsync(key, jsonPatchDocument, entityTransaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    private async Task PostUpdate(TEntity entity, CancellationToken cancellationToken)
    {
        if (cacheService is null)
        {
            return;
        }

        await cacheService.SetAsync(entity, cancellationToken);
    }

    protected override void DisposeManagedObjects() => updateClient?.Dispose();
}
