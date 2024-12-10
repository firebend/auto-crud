using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Implementations;

public class EfCustomFieldsCreateService<TKey, TEntity, TCustomFieldsTEntity>(
    IEntityFrameworkCreateClient<Guid, TCustomFieldsTEntity> createClient,
    ISessionTransactionManager transactionManager,
    IEntityCacheService<TKey, TEntity> cacheService = null)
    : BaseDisposable,
        ICustomFieldsCreateService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
    where TCustomFieldsTEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>, new()
{
    public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> customField,
        CancellationToken cancellationToken)
    {
        var transaction = await transactionManager.GetTransaction<TKey, TEntity>(cancellationToken);
        return await CreateAsync(rootEntityKey, customField, transaction, cancellationToken);
    }

    public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
        CustomFieldsEntity<TKey> customField,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
    {
        transactionManager.AddTransaction(entityTransaction);

        var customFieldsEntity = new TCustomFieldsTEntity { EntityId = rootEntityKey };

        customField.CopyPropertiesTo(customFieldsEntity);

        var added = await createClient.AddAsync(customFieldsEntity, entityTransaction, cancellationToken);

        if (cacheService != null)
        {
            await cacheService.RemoveAsync(rootEntityKey, cancellationToken);
        }

        var returnEntity = added?.ToCustomFields();

        return returnEntity;
    }

    protected override void DisposeManagedObjects()
        => createClient?.Dispose();
}
