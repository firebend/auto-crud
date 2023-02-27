using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;

public interface ICustomFieldsValidationService<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TVersion : class, IApiVersion
{
    Task<ModelStateResult<CustomFieldsEntity<TKey>>> ValidateAsync(CustomFieldsEntity<TKey> customField,
        CancellationToken cancellationToken);

    Task<ModelStateResult<JsonPatchDocument<CustomFieldsEntity<TKey>>>> ValidateAsync(JsonPatchDocument<CustomFieldsEntity<TKey>> patch,
        CancellationToken cancellationToken);
}
