using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
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
        private readonly IEntityFrameworkUpdateClient<Guid, CustomFieldsEntity<TKey, TEntity>> _updateClient;

        protected AbstractEfCustomFieldsUpdateService(IEntityFrameworkUpdateClient<Guid, CustomFieldsEntity<TKey, TEntity>> updateClient)
        {
            _updateClient = updateClient;
        }

        public async Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            CancellationToken cancellationToken = default)
        {
            entity.EntityId = rootEntityKey;
            var updated = await _updateClient
                .UpdateAsync(new CustomFieldsEntity<TKey, TEntity>(entity), cancellationToken)
                .ConfigureAwait(false);

            if (updated == null)
            {
                return null;
            }

            var retEntity = CustomFieldsEntity<TKey>.Create(updated);
            return retEntity;
        }

        public async Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default)
        {
            var operations = jsonPatchDocument
                .Operations
                .Select(x => new Operation<CustomFieldsEntity<TKey, TEntity>> {from = x.from, op = x.op, path = x.path, value = x.value})
                .ToList();

            var patch = new JsonPatchDocument<CustomFieldsEntity<TKey, TEntity>>(operations, new DefaultContractResolver());

            var updated = await _updateClient
                .UpdateAsync(key, patch, cancellationToken)
                .ConfigureAwait(false);

            if (updated == null)
            {
                return null;
            }

            var retEntity = CustomFieldsEntity<TKey>.Create(updated);
            return retEntity;
        }

        protected override void DisposeManagedObjects() => _updateClient?.Dispose();
    }
}
