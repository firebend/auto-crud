using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntitySoftDeleteService<TKey, TEntity>(
    IEntityUpdateService<TKey, TEntity> updateService,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable, IEntityDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, IActiveEntity
{
    protected virtual async Task<TEntity> DeleteInternalAsync(TKey key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        var patch = new JsonPatchDocument<TEntity>();

        patch.Add(x => x.IsDeleted, true);

        var deleted = await (entityTransaction is not null
            ? updateService.PatchAsync(key, patch, entityTransaction, cancellationToken)
            : updateService.PatchAsync(key, patch, cancellationToken));

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        return deleted;
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteInternalAsync(key, transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);
        return DeleteInternalAsync(key, entityTransaction, cancellationToken);
    }
}
