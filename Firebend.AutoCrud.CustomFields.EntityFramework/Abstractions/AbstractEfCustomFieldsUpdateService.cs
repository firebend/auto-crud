using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractEfCustomFieldsUpdateService<TKey, TEntity, TCustomFieldsEntity> : BaseDisposable, ICustomFieldsUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TCustomFieldsEntity : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>, new()
    {
        private readonly IEntityFrameworkUpdateClient<Guid, TCustomFieldsEntity> _updateClient;
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;

        protected AbstractEfCustomFieldsUpdateService(IEntityFrameworkUpdateClient<Guid, TCustomFieldsEntity> updateClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator)
        {
            _updateClient = updateClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
        }

        public Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> customField,
            CancellationToken cancellationToken = default)
            => UpdateAsync(rootEntityKey, customField, null, cancellationToken);

        public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> customField,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
            var efEntity = new TCustomFieldsEntity { EntityId = rootEntityKey };
            customField.CopyPropertiesTo(efEntity);

            var updated = await _updateClient
                .UpdateAsync(efEntity, entityTransaction, cancellationToken)
                .ConfigureAwait(false);

            var retEntity = updated?.ToCustomFields();
            return retEntity;
        }

        public Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default)
            => PatchAsync(rootEntityKey, key, jsonPatchDocument, null, cancellationToken);

        public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            var operations = jsonPatchDocument
                .Operations
                .Select(x => new Operation<TCustomFieldsEntity>
                {
                    from = x.from,
                    op = x.op,
                    path = x.path,
                    value = x.value
                })
                .ToList();

            var patch = new JsonPatchDocument<TCustomFieldsEntity>(operations, new DefaultContractResolver());

            var updated = await _updateClient
                .UpdateAsync(key, patch, entityTransaction, cancellationToken)
                .ConfigureAwait(false);

            var retEntity = updated?.ToCustomFields();
            return retEntity;
        }

        protected override void DisposeManagedObjects()
        {
            _updateClient?.Dispose();
            _customFieldsStorageCreator.Dispose();
        }
    }
}
