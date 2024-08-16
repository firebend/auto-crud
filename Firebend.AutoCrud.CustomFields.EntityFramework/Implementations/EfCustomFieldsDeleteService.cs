using System;
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

public class EfCustomFieldsDeleteService<TKey, TEntity, TCustomFieldsEntity> : BaseDisposable,
    ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
{
    private readonly ISessionTransactionManager _transactionManager;
    private readonly IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> _deleteClient;

    public EfCustomFieldsDeleteService(IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> deleteClient,
        ISessionTransactionManager transactionManager)
    {
        _deleteClient = deleteClient;
        _transactionManager = transactionManager;
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key,
        CancellationToken cancellationToken)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteAsync(rootEntityKey, key, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        _transactionManager.AddTransaction(entityTransaction);

        var deleted = await _deleteClient.DeleteAsync(key, entityTransaction, cancellationToken);

        var retDeleted = deleted?.ToCustomFields();

        return retDeleted;
    }

    protected override void DisposeManagedObjects() => _deleteClient?.Dispose();
}
