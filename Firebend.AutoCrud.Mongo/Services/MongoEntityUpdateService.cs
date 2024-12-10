using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntityUpdateService<TKey, TEntity>(
    IMongoUpdateClient<TKey, TEntity> updateClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable, IEntityUpdateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public async Task<TEntity> UpdateAsync(TEntity entity,
        CancellationToken cancellationToken)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);

        // Allow creating entities through PUT to make it easier to set the guid in the client
        // when creating new entities. ( ACID2.0 )
        var updated = await updateClient.UpsertAsync(entity, transaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        var updated = await updateClient.UpsertAsync(entity, entityTransaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public async Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        CancellationToken cancellationToken)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        var updated = await updateClient.UpdateAsync(key, jsonPatchDocument, transaction, cancellationToken);
        await PostUpdate(updated, cancellationToken);
        return updated;
    }

    public async Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
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

        await cacheService.RemoveAsync(entity.Id, cancellationToken);
    }
}
