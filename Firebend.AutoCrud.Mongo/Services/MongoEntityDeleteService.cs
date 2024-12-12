using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntityDeleteService<TKey, TEntity>(
    IMongoDeleteClient<TKey, TEntity> deleteClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable, IEntityDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private async Task<TEntity> DeleteInternalAsync(TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var deleted = await deleteClient.DeleteAsync(x => x.Id.Equals(key), transaction, cancellationToken);

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        return deleted;
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteInternalAsync(key, transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        if (key.Equals(default))
        {
            throw new ArgumentException("Key is invalid", nameof(key));
        }

        transactionManager.AddTransaction(entityTransaction);
        return DeleteInternalAsync(key, entityTransaction, cancellationToken);
    }
}
