using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
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

    public EntityFrameworkEntityDeleteService(IEntityFrameworkDeleteClient<TKey, TEntity> deleteClient,
        ISessionTransactionManager transactionManager)
    {
        _deleteClient = deleteClient;
        _transactionManager = transactionManager;
    }

    public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await _deleteClient.DeleteAsync(key, transaction, cancellationToken);
    }

    public Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        return _deleteClient.DeleteAsync(key, entityTransaction, cancellationToken);
    }

    protected override void DisposeManagedObjects() => _deleteClient?.Dispose();
}
