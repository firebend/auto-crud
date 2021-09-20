using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public abstract class AbstractCustomFieldsDeleteController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly ICustomFieldsDeleteService<TKey, TEntity> _deleteService;

        protected AbstractCustomFieldsDeleteController(IEntityKeyParser<TKey, TEntity> keyParser,
            ICustomFieldsDeleteService<TKey, TEntity> deleteService,
            IOptions<ApiBehaviorOptions> apiOptions) : base(keyParser, apiOptions)
        {
            _deleteService = deleteService;
        }

        [HttpDelete("{entityId}/custom-fields/{id:guid}")]
        [SwaggerOperation("Deletes a custom field for a given {entityName}")]
        [SwaggerResponse(201, "A custom field  was deleted successfully..")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> DeleteCustomFieldAsync(
            [Required][FromRoute] string entityId,
            [Required][FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_deleteService);

            if (!ModelState.IsValid)
            {
                return GetInvalidModelStateResult();
            }

            var rootKey = GetKey(entityId);

            if (rootKey == null)
            {
                return GetInvalidModelStateResult();
            }

            var result = await _deleteService
                .DeleteAsync(rootKey.Value, id, cancellationToken)
                .ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new { key = entityId, id });
            }

            return Ok(result);
        }
    }
}
