using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class AbstractEfCustomFieldsUpdateService<TKey, TEntity> : BaseDisposable, ICustomFieldsUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IEntityFrameworkUpdateClient<Guid, EfCustomFieldsModel<TKey, TEntity>> _updateClient;
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;

        protected AbstractEfCustomFieldsUpdateService(IEntityFrameworkUpdateClient<Guid, EfCustomFieldsModel<TKey, TEntity>> updateClient,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator)
        {
            _updateClient = updateClient;
            _customFieldsStorageCreator = customFieldsStorageCreator;
        }

        public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
            entity.EntityId = rootEntityKey;
            var updated = await _updateClient
                .UpdateAsync(new EfCustomFieldsModel<TKey, TEntity>(entity), cancellationToken)
                .ConfigureAwait(false);

            var retEntity = updated?.ToCustomFields();
            return retEntity;
        }

        public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default)
        {
            await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);

            var operations = jsonPatchDocument
                .Operations
                .Select(x => new Operation<EfCustomFieldsModel<TKey, TEntity>> {from = x.from, op = x.op, path = x.path, value = x.value})
                .ToList();

            var patch = new JsonPatchDocument<EfCustomFieldsModel<TKey, TEntity>>(operations, new DefaultContractResolver());

            var updated = await _updateClient
                .UpdateAsync(key, patch, cancellationToken)
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
