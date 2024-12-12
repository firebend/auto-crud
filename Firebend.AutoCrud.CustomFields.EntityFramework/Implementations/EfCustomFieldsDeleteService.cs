using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Implementations;

public class EfCustomFieldsDeleteService<TKey, TEntity, TCustomFieldsEntity>(
    IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> deleteClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable,
        ICustomFieldsDeleteService<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
{
    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await DeleteAsync(rootEntityKey, key, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
        Guid key,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);

        var deleted = await deleteClient.DeleteAsync(key, entityTransaction, cancellationToken);

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(rootEntityKey, cancellationToken);
        }

        var retDeleted = deleted?.ToCustomFields();

        return retDeleted;
    }

    protected override void DisposeManagedObjects() => deleteClient?.Dispose();
}
