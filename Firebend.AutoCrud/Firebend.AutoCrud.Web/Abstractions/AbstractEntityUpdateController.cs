using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractEntityUpdateController<TKey, TEntity> : ControllerBase
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private readonly IEntityKeyParser<TKey, TEntity> _entityKeyParser;
        private readonly IEntityValidationService<TKey, TEntity> _entityValidationService;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;

        protected AbstractEntityUpdateController(IEntityUpdateService<TKey, TEntity> updateService,
            IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IEntityValidationService<TKey, TEntity> entityValidationService)
        {
            _updateService = updateService;
            _readService = readService;
            _entityKeyParser = entityKeyParser;
            _entityValidationService = entityValidationService;
        }

        [HttpPut("{id}")]
        [SwaggerOperation("Updates an entity")]
        [SwaggerResponse(200, "Updates the entity with a given key.")]
        [SwaggerResponse(404, "The entity with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Put([FromRoute] string id,
            [FromBody] TEntity body,
            CancellationToken cancellationToken)
        {
            var key = _entityKeyParser.ParseKey(id);

            if (key.Equals(default) || key.Equals(null))
            {
                ModelState.AddModelError(nameof(id), "An id is required");
                return BadRequest(ModelState);
            }

            if (body == null || body.Id.Equals(default) || body.Id.Equals(null))
            {
                ModelState.AddModelError(nameof(body), "The entity body has no id provided.");
                return BadRequest(ModelState);
            }

            if (!key.Equals(body.Id))
            {
                ModelState.AddModelError(nameof(id), "The id provided in the url does not match the id in the body.");
                return BadRequest(ModelState);
            }

            var isValid = await _entityValidationService
                .ValidateAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (!isValid.WasSuccessful)
            {
                foreach (var modelError in isValid.Errors)
                {
                    ModelState.AddModelError(modelError.PropertyPath, modelError.Error);
                }

                return BadRequest(ModelState);
            }

            var entity = await _updateService
                .UpdateAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new {key = id});
            }

            return Ok(entity);
        }

        [HttpPatch("{id}")]
        [SwaggerOperation("Updates an entity using a JSON Patch Document")]
        [SwaggerResponse(200, "An entity with the given key.")]
        [SwaggerResponse(404, "The entity with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Patch([FromRoute] string id,
            [FromBody] [Required] JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken)
        {
            if (patch == null)
            {
                ModelState.AddModelError(nameof(patch), "A valid patch document is required.");

                return BadRequest(ModelState);
            }

            var key = _entityKeyParser.ParseKey(id);

            if (key.Equals(default) || key.Equals(null))
            {
                ModelState.AddModelError(nameof(id), "An id is required");

                return BadRequest(ModelState);
            }

            var entity = await _readService
                .GetByKeyAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new {id});
            }

            var original = entity.Clone();

            patch.ApplyTo(entity, ModelState);

            if (!ModelState.IsValid || !TryValidateModel(entity))
            {
                return BadRequest(ModelState);
            }

            var isValid = await _entityValidationService
                .ValidateAsync(original, entity, patch, cancellationToken)
                .ConfigureAwait(false);

            if (!isValid.WasSuccessful)
            {
                foreach (var modelError in isValid.Errors)
                {
                    ModelState.AddModelError(modelError.PropertyPath, modelError.Error);
                }

                return BadRequest(ModelState);
            }

            if (isValid.Model != null)
            {
                entity = isValid.Model;
            }

            var update = await _updateService
                .UpdateAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            if (update != null)
            {
                return Ok(update);
            }

            return NotFound(new {id});
        }
    }
}