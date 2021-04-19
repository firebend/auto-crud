using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsUpdateService<TKey, TEntity> : IDisposable
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            CancellationToken cancellationToken = default);

        Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default);

        Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default);

        Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default);
    }
}
