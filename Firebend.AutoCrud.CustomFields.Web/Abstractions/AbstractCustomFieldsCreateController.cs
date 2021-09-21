using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.Web.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public abstract class AbstractCustomFieldsCreateController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly ICustomFieldsCreateService<TKey, TEntity> _createService;

        protected AbstractCustomFieldsCreateController(IEntityKeyParser<TKey, TEntity> keyParser,
            ICustomFieldsCreateService<TKey, TEntity> createService,
            IOptions<ApiBehaviorOptions> apiOptions) : base(keyParser, apiOptions)
        {
            _createService = createService;
        }

        [HttpPost("{entityId}/custom-fields")]
        [SwaggerOperation("Creates a custom field for a given {entityName}")]
        [SwaggerResponse(200, "A custom field  was created successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public async Task<ActionResult<CustomFieldsEntity<TKey>>> CreateCustomFieldAsync(
            [FromRoute] string entityId,
            [FromBody] CustomAttributeViewModelCreate viewModel,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_createService);

            if (!ModelState.IsValid || !TryValidateModel(viewModel))
            {
                return GetInvalidModelStateResult();
            }

            var rootKey = GetKey(entityId);

            if (rootKey == null)
            {
                return GetInvalidModelStateResult();
            }

            var entity = new CustomFieldsEntity<TKey> { Key = viewModel.Key, Value = viewModel.Value, EntityId = rootKey.Value };

            if (!ModelState.IsValid || !TryValidateModel(entity))
            {
                return GetInvalidModelStateResult();
            }

            var result = await _createService
                .CreateAsync(rootKey.Value, entity, cancellationToken).ConfigureAwait(false);

            if (result == null)
            {
                return NotFound(new { key = entityId });
            }

            return Ok(result);
        }
    }
}
