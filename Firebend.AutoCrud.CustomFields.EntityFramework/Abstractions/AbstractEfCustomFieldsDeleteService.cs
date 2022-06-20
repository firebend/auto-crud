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

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;

public abstract class AbstractEfCustomFieldsDeleteService<TKey, TEntity, TCustomFieldsEntity> : BaseDisposable,
    ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
{
    private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;
    private readonly ISessionTransactionManager _transactionManager;
    private readonly IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> _deleteClient;

    protected AbstractEfCustomFieldsDeleteService(IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> deleteClient,
        ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator,
        ISessionTransactionManager transactionManager)
    {
        _deleteClient = deleteClient;
        _customFieldsStorageCreator = customFieldsStorageCreator;
        _transactionManager = transactionManager;
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteAsync(rootEntityKey, key, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        _transactionManager.AddTransaction(entityTransaction);
        await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

        var deleted = await _deleteClient
            .DeleteAsync(key, entityTransaction, cancellationToken)
            .ConfigureAwait(false);

        var retDeleted = deleted?.ToCustomFields();

        return retDeleted;
    }

    protected override void DisposeManagedObjects()
    {
        _deleteClient?.Dispose();
        _customFieldsStorageCreator?.Dispose();
    }
}
