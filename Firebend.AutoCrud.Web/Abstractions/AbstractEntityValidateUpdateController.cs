using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityValidateUpdateController<TKey, TEntity, TUpdateViewModel, TUpdateViewModelBody, TReadViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
        where TReadViewModel : class
        where TUpdateViewModel : class
        where TUpdateViewModelBody : class
    {

        private readonly IUpdateViewModelMapper<TKey, TEntity, TUpdateViewModel, TUpdateViewModelBody> _updateViewModelMapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TReadViewModel> _readViewModelMapper;

        protected AbstractEntityValidateUpdateController(
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IUpdateViewModelMapper<TKey, TEntity, TUpdateViewModel, TUpdateViewModelBody> updateViewModelMapper,
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readViewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(entityKeyParser, apiOptions)
        {
            _updateViewModelMapper = updateViewModelMapper;
            _readViewModelMapper = readViewModelMapper;
        }

        [HttpPut("{id}/validate")]
        [SwaggerOperation("Shallow Validation on {entityNamePlural}")]
        [SwaggerResponse(200, "The {entityName}  was validated successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> ValidatePutAsync(
            [Required][FromRoute] string id,
            [Required] TUpdateViewModel body,
            CancellationToken cancellationToken)
        {
            var key = GetKey(id);

            if (!key.HasValue)
            {
                return GetInvalidModelStateResult();
            }

            var entityUpdate = await this.ValidateModel(body, _updateViewModelMapper, cancellationToken);

            if (!ModelState.IsValid)
            {
                return GetInvalidModelStateResult();
            }

            if (IsCustomFieldsEntity() && HasCustomFieldsPopulated(entityUpdate))
            {
                ModelState.AddModelError(nameof(body), "Modifying an entity's custom fields is not allowed in this endpoint. Please use the entity's custom fields endpoints.");

                return GetInvalidModelStateResult();
            }

            entityUpdate.Id = key.Value;

            var mapped = await _readViewModelMapper
                .ToAsync(entityUpdate, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }
    }
}
