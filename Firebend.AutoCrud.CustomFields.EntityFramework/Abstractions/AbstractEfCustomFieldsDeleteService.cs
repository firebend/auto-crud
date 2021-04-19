using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractEfCustomFieldsDeleteService<TKey, TEntity, TCustomFieldsEntity> : BaseDisposable, ICustomFieldsDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>
    {
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;
        private readonly IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> _deleteClient;

        protected AbstractEfCustomFieldsDeleteService(IEntityFrameworkDeleteClient<Guid, TCustomFieldsEntity> deleteClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator)
        {
            _deleteClient = deleteClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
        }

        public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key, CancellationToken cancellationToken = default)
            => DeleteAsync(rootEntityKey, key, null, cancellationToken);

        public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
            Guid key,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
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
}
