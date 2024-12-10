using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Services;

public class EntityFrameworkEntityDeleteService<TKey, TEntity> : BaseDisposable,
    IEntityDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IEntityFrameworkDeleteClient<TKey, TEntity> _deleteClient;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly IEntityCacheService<TKey, TEntity> _cacheService;

    public EntityFrameworkEntityDeleteService(IEntityFrameworkDeleteClient<TKey, TEntity> deleteClient,
        ISessionTransactionManager transactionManager,
        IEntityCacheService<TKey, TEntity> cacheService = null)
    {
        _deleteClient = deleteClient;
        _transactionManager = transactionManager;
        _cacheService = cacheService;
    }

    private async Task<TEntity> DeleteInternalAsync(TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var deleted = await _deleteClient.DeleteAsync(key, transaction, cancellationToken);

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync(key, cancellationToken);
        }

        return deleted;
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteInternalAsync(key, transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return DeleteInternalAsync(key, entityTransaction, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _deleteClient?.Dispose();
}
