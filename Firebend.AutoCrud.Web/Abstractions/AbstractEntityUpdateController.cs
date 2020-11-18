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
    public abstract class AbstractEntityUpdateController<TKey, TEntity, TUpdateViewModel, TReadViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
        where TReadViewModel : class
        where TUpdateViewModel : class
    {
        private readonly IEntityValidationService<TKey, TEntity> _entityValidationService;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;
        private readonly IUpdateViewModelMapper<TKey, TEntity, TUpdateViewModel> _updateViewModelMapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TReadViewModel> _readViewModelMapper;

        protected AbstractEntityUpdateController(IEntityUpdateService<TKey, TEntity> updateService,
            IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IEntityValidationService<TKey, TEntity> entityValidationService,
            IUpdateViewModelMapper<TKey, TEntity, TUpdateViewModel> updateViewModelMapper,
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readViewModelMapper) : base(entityKeyParser)
        {
            _updateService = updateService;
            _readService = readService;
            _entityValidationService = entityValidationService;
            _updateViewModelMapper = updateViewModelMapper;
            _readViewModelMapper = readViewModelMapper;
        }

        [HttpPut("{id}")]
        [SwaggerOperation("Updates {entityNamePlural}")]
        [SwaggerResponse(200, "The {entityName} with a given key was updated.")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Put(
            [Required][FromRoute] string id,
            [Required] TUpdateViewModel body,
            CancellationToken cancellationToken)
        {
            var key = GetKey(id);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            if (body == null)
            {
                ModelState.AddModelError(nameof(body), "A body is required");

                return BadRequest(ModelState);
            }

            var entityUpdate = await _updateViewModelMapper
                .FromAsync(body, cancellationToken)
                .ConfigureAwait(false);

            entityUpdate.Id = key.Value;

            var isValid = await _entityValidationService
                .ValidateAsync(entityUpdate, cancellationToken)
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
                .UpdateAsync(entityUpdate, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new { id });
            }

            var mapped = await _readViewModelMapper
                .ToAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }

        [HttpPatch("{id}")]
        [SwaggerOperation("Updates {entityNamePlural} using a JSON Patch Document")]
        [SwaggerResponse(200, "The {entityName} with the given key was updated.")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Patch(
            [Required][FromRoute] string id,
            [Required][FromBody] JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken)
        {
            if (patch == null)
            {
                ModelState.AddModelError(nameof(patch), "A valid patch document is required.");

                return BadRequest(ModelState);
            }

            var key = GetKey(id);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            var entity = await _readService
                .GetByKeyAsync(key.Value, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new { id });
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
                var mapped = await _readViewModelMapper
                    .ToAsync(update, cancellationToken)
                    .ConfigureAwait(false);

                return Ok(mapped);
            }

            return NotFound(new { id });
        }
    }
}
