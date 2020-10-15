#region

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

#endregion

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityCreateController<TKey, TEntity> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityCreateService<TKey, TEntity> _createService;
        private readonly IEntityValidationService<TKey, TEntity> _entityValidationService;

        public AbstractEntityCreateController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity> entityValidationService)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
        }

        [HttpPost]
        [SwaggerOperation("Creates an entity")]
        [SwaggerResponse(201, "Creates an entity.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Post([FromBody] TEntity body,
            CancellationToken cancellationToken)
        {
            var isValid = await _entityValidationService
                .ValidateAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (!isValid.WasSuccessful)
            {
                foreach (var modelError in isValid.Errors) ModelState.AddModelError(modelError.PropertyPath, modelError.Error);
                return BadRequest(ModelState);
            }

            if (isValid.Model != null) body = isValid.Model;

            var entity = await _createService
                .CreateAsync(body, cancellationToken)
                .ConfigureAwait(false);

            return Created($"{Request.Path.Value}/{entity.Id}", entity);
        }

        [HttpPost]
        [Route("multiple")]
        [SwaggerOperation("Creates multiple entities")]
        [SwaggerResponse(201, "Multiple entities created.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> PostMultiple([FromBody] TEntity[] body,
            CancellationToken cancellationToken)
        {
            var createdEntities = new List<TEntity>();
            var errorEntities = new List<ModelStateResult<TEntity>>();

            foreach (var toCreate in body)
            {
                var entityToCreate = toCreate;
                var isValid = await _entityValidationService
                    .ValidateAsync(entityToCreate, cancellationToken)
                    .ConfigureAwait(false);

                if (!isValid.WasSuccessful)
                {
                    isValid.Model = toCreate;
                    errorEntities.Add(isValid);
                }

                if (isValid.Model != null) entityToCreate = isValid.Model;

                var entity = await _createService
                    .CreateAsync(entityToCreate, cancellationToken)
                    .ConfigureAwait(false);

                createdEntities.Add(entity);
            }

            if (createdEntities.Count > 0) return Ok(new {created = createdEntities, errors = errorEntities});

            if (errorEntities.Count > 0) return BadRequest(new {errors = errorEntities});

            return BadRequest();
        }
    }
}