using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Web.Sample.ValidationServices;

public class CustomFieldValidationService<TKey, TEntity, TVersion> : ICustomFieldsValidationService<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TVersion : class, IApiVersion
{
    private readonly ICustomFieldsReadService<TKey, TEntity> _customFieldsReadService;

    public CustomFieldValidationService(ICustomFieldsReadService<TKey, TEntity> customFieldsReadService)
    {
        _customFieldsReadService = customFieldsReadService;
    }

    public async Task<ModelStateResult<CustomFieldsEntity<TKey>>> ValidateAsync(CustomFieldsEntity<TKey> customField,
        CancellationToken cancellationToken)
    {
        var modelState = new ModelStateResult<CustomFieldsEntity<TKey>> { WasSuccessful = true, Model = customField };

        if (customField.Id != Guid.Empty)
        {
            return modelState;
        }
        var customFields = await _customFieldsReadService.GetAllAsync(customField.EntityId, cancellationToken);
        if (customFields.HasValues() && customFields.Count >= 10)
        {
            modelState.AddError(nameof(customFields),
                $"Only 10 custom fields allowed per {typeof(TEntity).Name}");
            return modelState;
        }

        return modelState;
    }

    public Task<ModelStateResult<JsonPatchDocument<CustomFieldsEntity<TKey>>>> ValidateAsync(
        JsonPatchDocument<CustomFieldsEntity<TKey>> patch,
        CancellationToken cancellationToken)
    {

        var modelState =
            new ModelStateResult<JsonPatchDocument<CustomFieldsEntity<TKey>>> { WasSuccessful = true, Model = patch };

        var badTranslations = patch.Operations.Where(x =>
            x.path.EndsWith(nameof(CustomFieldsEntity<TKey>.Value)) && x.value is string valString &&
            valString.Equals("All your base are belong to us!")).ToList();

        if (badTranslations.Any())
        {
            foreach (var operation in badTranslations)
            {
                operation.value = "With the help of Federation government forces, CATS has taken all of your bases.";
            }
        }

        return Task.FromResult(modelState);
    }
}
