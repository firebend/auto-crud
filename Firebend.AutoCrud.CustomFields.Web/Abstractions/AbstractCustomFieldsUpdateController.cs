using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.Web.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public class AbstractCustomAttributeUpdateController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly ICustomFieldsUpdateService<TKey> _updateService;

        public AbstractCustomAttributeUpdateController(IEntityKeyParser<TKey, TEntity> keyParser,
            ICustomFieldsUpdateService<TKey> updateService) : base(keyParser)
        {
            _updateService = updateService;
        }

        [HttpPost("{entityId}/custom-fields/{id:guid}")]
        [SwaggerOperation("Updates a custom field for a given {entityName}")]
        [SwaggerResponse(201, "A custom field  was updated successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> PostAsync(
            [Required] [FromRoute] string key,
            [Required] [FromRoute] Guid id,
            [FromBody] CustomAttributeViewModelCreate viewModel,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_updateService);

            if (ModelState.IsValid || !TryValidateModel(viewModel))
            {
                return BadRequest(ModelState);
            }

            var rootKey = GetKey(key);

            if (rootKey == null)
            {
                return BadRequest(ModelState);
            }

            var entity = new CustomFieldsEntity<TKey> {Key = viewModel.Key, Value = viewModel.Value, EntityId = rootKey.Value, Id = id};

            if (ModelState.IsValid || !TryValidateModel(entity))
            {
                return BadRequest(ModelState);
            }

            var result = await _updateService
                .UpdateAsync(rootKey.Value, entity, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new {key, id});
            }

            return Ok(result);
        }

        [HttpPatch("{entityId}/custom-fields/{id:guid}")]
        [SwaggerOperation("Patches a custom field for a given {entityName}")]
        [SwaggerResponse(201, "A custom field was patched successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> PatchAsync(
            [Required] [FromRoute] string key,
            [Required] [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<CustomFieldsEntity<TKey>> patchDocument,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_updateService);

            if (ModelState.IsValid || !TryValidateModel(patchDocument))
            {
                return BadRequest(ModelState);
            }

            var rootKey = GetKey(key);

            if (rootKey == null)
            {
                return BadRequest(ModelState);
            }

            var result = await _updateService
                .PatchAsync(rootKey.Value, id, patchDocument, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new {key, id});
            }

            return Ok(result);
        }
    }
}
