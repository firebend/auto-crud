using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.CustomFields.Web.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public abstract class
        AbstractCustomFieldsUpdateController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly ICustomFieldsUpdateService<TKey, TEntity> _updateService;

        protected AbstractCustomFieldsUpdateController(IEntityKeyParser<TKey, TEntity> keyParser,
            ICustomFieldsUpdateService<TKey, TEntity> updateService,
            IOptions<ApiBehaviorOptions> apiOptions) : base(keyParser, apiOptions)
        {
            _updateService = updateService;
        }

        [HttpPut("{entityId}/custom-fields/{id:guid}")]
        [SwaggerOperation("Updates a custom field for a given {entityName}")]
        [SwaggerResponse(200, "A custom field  was updated successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> CustomFieldsUpdatePutAsync(
            [Required] [FromRoute] string entityId,
            [Required] [FromRoute] Guid id,
            [FromBody] CustomFieldViewModelCreate viewModel,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_updateService);

            if (!ModelState.IsValid || !TryValidateModel(viewModel))
            {
                return GetInvalidModelStateResult();
            }

            var rootKey = GetKey(entityId);

            if (rootKey == null)
            {
                return GetInvalidModelStateResult();
            }

            var entity = new CustomFieldsEntity<TKey>
            {
                Key = viewModel.Key, Value = viewModel.Value, EntityId = rootKey.Value, Id = id
            };

            if (!ModelState.IsValid || !TryValidateModel(entity))
            {
                return GetInvalidModelStateResult();
            }

            var result = await _updateService
                .UpdateAsync(rootKey.Value, entity, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new {key = entityId, id});
            }

            return Ok(result);
        }

        [HttpPatch("{entityId}/custom-fields/{id:guid}")]
        [SwaggerOperation("Patches a custom field for a given {entityName}")]
        [SwaggerResponse(201, "A custom field was patched successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> CustomFieldsUpdatePatchAsync(
            [Required] [FromRoute] string entityId,
            [Required] [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<CustomFieldViewModelCreate> patchDocument,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_updateService);

            if (!ModelState.IsValid)
            {
                return GetInvalidModelStateResult();
            }

            if (!patchDocument.ValidatePatchModel(out var patchValidationResults))
            {
                foreach (var validationResult in patchValidationResults)
                {
                    ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage!);
                }

                return GetInvalidModelStateResult();
            }

            var rootKey = GetKey(entityId);

            if (rootKey == null)
            {
                return GetInvalidModelStateResult();
            }

            var entityPatchDocument = new JsonPatchDocument<CustomFieldsEntity<TKey>>();
            if (!patchDocument.TryCopyTo(entityPatchDocument, out var patchError))
            {
                ModelState.AddModelError(nameof(JsonPatchDocument<CustomFieldViewModelCreate>),
                    $"Unable to make patch for {nameof(CustomFieldsEntity<TKey>)} using {nameof(CustomFieldViewModelCreate)}. {patchError}");
                return GetInvalidModelStateResult();
            }

            var result = await _updateService
                .PatchAsync(rootKey.Value, id, entityPatchDocument, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new {key = entityId, id});
            }

            return Ok(result);
        }
    }
}
