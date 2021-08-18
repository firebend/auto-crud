using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
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
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readViewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(entityKeyParser, apiOptions)
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
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> Put(
            [Required][FromRoute] string id,
            [Required] TUpdateViewModel body,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_readService);
            Response.RegisterForDispose(_updateService);

            var key = GetKey(id);

            if (!key.HasValue)
            {
                return GetInvalidModelStateResult();
            }

            if (body == null)
            {
                ModelState.AddModelError(nameof(body), "A body is required");

                return GetInvalidModelStateResult();
            }

            var entityUpdate = await _updateViewModelMapper
                .FromAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (entityUpdate == null)
            {
                throw new Exception("Update view model mapper did not map to entity.");
            }

            if (!ModelState.IsValid || !TryValidateModel(entityUpdate))
            {
                return GetInvalidModelStateResult();
            }

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

                return GetInvalidModelStateResult();
            }

            TEntity entity;

            try
            {
                entity = await _updateService
                    .UpdateAsync(entityUpdate, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (AutoCrudEntityException ex)
            {
                if (ex.PropertyErrors != null)
                {
                    foreach (var (property, error) in ex.PropertyErrors)
                    {
                        ModelState.AddModelError(property, error);
                    }

                }

                return GetInvalidModelStateResult();
            }

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
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> Patch(
            [Required][FromRoute] string id,
            [Required][FromBody] JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_readService);
            Response.RegisterForDispose(_updateService);

            if (patch == null)
            {
                ModelState.AddModelError(nameof(patch), "A valid patch document is required.");

                return GetInvalidModelStateResult();
            }

            if (patch.Operations.Any(x => x.path.Equals($"/{nameof(IEntity<Guid>.Id)}", StringComparison.InvariantCultureIgnoreCase)))
            {
                ModelState.AddModelError(nameof(patch), "Modifying the entity's id during patch is not allowed.");

                return GetInvalidModelStateResult();
            }

            var key = GetKey(id);

            if (!key.HasValue)
            {
                return GetInvalidModelStateResult();
            }

            var entity = await _readService
                .GetByKeyAsync(key.Value, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new { id });
            }

            var original = entity.Clone();

            ApplyTo(patch, entity, ModelState, string.Empty);

            if (!ModelState.IsValid || !TryValidateModel(entity))
            {
                return GetInvalidModelStateResult();
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

                return GetInvalidModelStateResult();
            }

            if (isValid.Model != null)
            {
                entity = isValid.Model;
            }

            TEntity update;

            try
            {
                update = await _updateService
                    .UpdateAsync(entity, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (AutoCrudEntityException ex)
            {
                if (ex.PropertyErrors != null)
                {
                    foreach (var (property, error) in ex.PropertyErrors)
                    {
                        ModelState.AddModelError(property, error);
                    }

                }

                return GetInvalidModelStateResult();
            }

            if (update == null)
            {
                return NotFound(new { id });
            }

            var mapped = await _readViewModelMapper
                .ToAsync(update, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }

        //********************************************
        // Author: JMA
        // Date: 2021-01-15 06:08:57
        // Comment: yoink from here https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.NewtonsoftJson/src/JsonPatchExtensions.cs
        // we are doing this because we want to keep this package netstandard2.1
        // if we incorporate the package that includes this extension we have to force a specific version of asp net mvc i.e 3.1 or 5.0
        //*******************************************
        private static void ApplyTo<T>(
            JsonPatchDocument<T> patchDoc,
            T objectToApplyTo,
            ModelStateDictionary modelState,
            string prefix) where T : class
        {
            if (patchDoc == null)
            {
                throw new ArgumentNullException(nameof(patchDoc));
            }

            if (objectToApplyTo == null)
            {
                throw new ArgumentNullException(nameof(objectToApplyTo));
            }

            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            patchDoc.ApplyTo(objectToApplyTo, jsonPatchError =>
            {
                var affectedObjectName = jsonPatchError.AffectedObject.GetType().Name;
                var key = string.IsNullOrEmpty(prefix) ? affectedObjectName : prefix + "." + affectedObjectName;

                modelState.TryAddModelError(key, jsonPatchError.ErrorMessage);
            });
        }
    }
}
