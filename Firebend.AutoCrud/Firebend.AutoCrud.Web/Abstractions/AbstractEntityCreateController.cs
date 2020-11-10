using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    using Interfaces;

    [ApiController]
    public abstract class AbstractEntityCreateController<TKey, TEntity, TViewModel> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IEntityCreateService<TKey, TEntity> _createService;
        private readonly IEntityValidationService<TKey, TEntity> _entityValidationService;
        private readonly IViewModelMapper<TKey, TEntity, TViewModel> _mapper;

        public AbstractEntityCreateController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity> entityValidationService,
            IViewModelMapper<TKey, TEntity, TViewModel> mapper)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _mapper = mapper;
        }

        [HttpPost]
        [SwaggerOperation("Creates {entityNamePlural}")]
        [SwaggerResponse(201, "A {entityName} was created successfully..")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Post(
            [FromBody] TViewModel body,
            CancellationToken cancellationToken)
        {
            if (body == null)
            {
                ModelState.AddModelError("body", "A body is required");
                return BadRequest(ModelState);
            }

            var entity = await _mapper.FromAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (!TryValidateModel(entity))
            {
                return BadRequest(ModelState);
            }

            var isValid = await _entityValidationService
                .ValidateAsync(entity, cancellationToken)
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

            var created = await _createService
                .CreateAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            var createdViewModel = await _mapper
                .ToAsync(created, cancellationToken)
                .ConfigureAwait(false);

            return Created($"{Request.Path.Value}/{created.Id}", createdViewModel);
        }

        [HttpPost]
        [Route("multiple")]
        [SwaggerOperation("Creates multiple {entityNamePlural}")]
        [SwaggerResponse(201, "Multiple {entityNamePlural} were created successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> PostMultiple(
            [FromBody] TViewModel[] body,
            CancellationToken cancellationToken)
        {
            if (body == null || body.Length <= 0)
            {
                ModelState.AddModelError("body", "A body is required");
                return BadRequest(ModelState);
            }

            var createdEntities = new List<TViewModel>();
            var errorEntities = new List<ModelStateResult<TEntity>>();

            foreach (var toCreate in body)
            {
                var entityToCreate = await _mapper
                    .FromAsync(toCreate, cancellationToken)
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

                var mappedEntity = await _mapper
                    .ToAsync(entity, cancellationToken)
                    .ConfigureAwait(false);

                createdEntities.Add(mappedEntity);
            }

            if (createdEntities.Count > 0)
            {
                return Ok(new
                {
                    created = createdEntities,
                    errors = errorEntities
                });
            }

            if (errorEntities.Count > 0)
            {
                return BadRequest(new
                {
                    errors = errorEntities
                });
            }

            return BadRequest();
        }
    }
}
