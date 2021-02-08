using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractEfCustomFieldsCreateService<TKey, TEntity, TCustomFieldsTEntity> : BaseDisposable, ICustomFieldsCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        where TCustomFieldsTEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>, new()
    {
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;
        private readonly IEntityFrameworkCreateClient<Guid, TCustomFieldsTEntity> _createClient;

        protected AbstractEfCustomFieldsCreateService(IEntityFrameworkCreateClient<Guid, TCustomFieldsTEntity> createClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator)
        {
            _createClient = createClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
        }

        public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> entity, CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            var customFieldsEntity = new TCustomFieldsTEntity
            {
                EntityId = rootEntityKey
            };

            entity.CopyPropertiesTo(customFieldsEntity);

            var added = await _createClient.AddAsync(customFieldsEntity, cancellationToken).ConfigureAwait(false);

            var returnEntity = added?.ToCustomFields();

            return returnEntity;
        }

        protected override void DisposeManagedObjects()
        {
            _createClient?.Dispose();
            _customFieldsStorageCreator?.Dispose();
        }
    }
}
