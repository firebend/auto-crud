using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public abstract class
    DefaultCustomFieldsValidationService<TKey, TEntity> : ICustomFieldsValidationService<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
{
    public Task<ModelStateResult<CustomFieldsEntity<TKey>>> ValidateAsync(CustomFieldsEntity<TKey> customField,
        CancellationToken cancellationToken)
        => Task.FromResult(ModelStateResult.Success(customField));

    public Task<ModelStateResult<JsonPatchDocument<CustomFieldsEntity<TKey>>>> ValidateAsync(
        JsonPatchDocument<CustomFieldsEntity<TKey>> patch,
        CancellationToken cancellationToken)
        => Task.FromResult(ModelStateResult.Success(patch));
}
