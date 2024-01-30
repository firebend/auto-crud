using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Mongo.Services;

public class MongoEntitySoftDeleteService<TKey, TEntity> : BaseDisposable, IEntityDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, IActiveEntity
{
    private readonly IEntityUpdateService<TKey, TEntity> _updateService;
    private readonly ISessionTransactionManager _transactionManager;

    public MongoEntitySoftDeleteService(IEntityUpdateService<TKey, TEntity> updateService,
        ISessionTransactionManager transactionManager)
    {
        _updateService = updateService;
        _transactionManager = transactionManager;
    }

    protected virtual Task<TEntity> DeleteInternalAsync(TKey key,
        IEntityTransaction entityTransaction = null,
        CancellationToken cancellationToken = default)
    {
        var patch = new JsonPatchDocument<TEntity>();

        patch.Add(x => x.IsDeleted, true);

        return entityTransaction != null
            ? _updateService.PatchAsync(key, patch, entityTransaction, cancellationToken)
            : _updateService.PatchAsync(key, patch, cancellationToken);
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteInternalAsync(key, transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return DeleteInternalAsync(key, entityTransaction, cancellationToken);
    }
}
