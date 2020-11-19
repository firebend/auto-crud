using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractEntityCreateMultipleController<TKey, TEntity, TMultipleViewModelWrapper, TMultipleViewModel, TReadViewModel> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TMultipleViewModel : class
        where TReadViewModel : class
        where TMultipleViewModelWrapper : IMultipleEntityViewModel<TMultipleViewModel>
    {
        private readonly IEntityCreateService<TKey, TEntity> _createService;
        private readonly IEntityValidationService<TKey, TEntity> _entityValidationService;
        private readonly ICreateMultipleViewModelMapper<TKey, TEntity, TMultipleViewModelWrapper, TMultipleViewModel> _multipleMapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TReadViewModel> _readMapper;

        protected AbstractEntityCreateMultipleController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity> entityValidationService,
            ICreateMultipleViewModelMapper<TKey, TEntity, TMultipleViewModelWrapper, TMultipleViewModel> multipleMapper,
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readMapper)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _multipleMapper = multipleMapper;
            _readMapper = readMapper;
        }

        [HttpPost]
        [Route("multiple")]
        [SwaggerOperation("Creates multiple {entityNamePlural}")]
        [SwaggerResponse(201, "Multiple {entityNamePlural} were created successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> PostMultiple(
            TMultipleViewModelWrapper body,
            CancellationToken cancellationToken)
        {
            if (body?.Entities?.IsEmpty() ?? true)
            {
                ModelState.AddModelError("body", "A body is required");
                return BadRequest(ModelState);
            }

            var createdEntities = new List<TReadViewModel>();
            var errorEntities = new List<ModelStateResult<TEntity>>();

            foreach (var toCreate in body.Entities)
            {
                var entityToCreate = await _multipleMapper
                    .FromAsync(body, toCreate, cancellationToken)
                    .ConfigureAwait(false);

                var isValid = await _entityValidationService
                    .ValidateAsync(entityToCreate, cancellationToken)
                    .ConfigureAwait(false);

                if (!isValid.WasSuccessful)
                {
                    isValid.Model = entityToCreate;
                    errorEntities.Add(isValid);
                }

                if (isValid.Model != null)
                {
                    entityToCreate = isValid.Model;
                }

                var entity = await _createService
                    .CreateAsync(entityToCreate, cancellationToken)
                    .ConfigureAwait(false);

                var mappedEntity = await _readMapper
                    .ToAsync(entity, cancellationToken)
                    .ConfigureAwait(false);

                createdEntities.Add(mappedEntity);
            }

            if (createdEntities.Count > 0)
            {
                return Ok(new { created = createdEntities, errors = errorEntities });
            }

            if (errorEntities.Count > 0)
            {
                return BadRequest(new { errors = errorEntities });
            }

            return BadRequest();
        }
    }
}
