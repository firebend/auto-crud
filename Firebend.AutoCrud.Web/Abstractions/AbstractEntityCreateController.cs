using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityCreateController<TKey, TEntity, TCreateViewModel, TReadViewModel> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TCreateViewModel : class
        where TReadViewModel : class
    {
        private IEntityCreateService<TKey, TEntity> _createService;
        private IEntityValidationService<TKey, TEntity> _entityValidationService;
        private ICreateViewModelMapper<TKey, TEntity, TCreateViewModel> _mapper;
        private IReadViewModelMapper<TKey, TEntity, TReadViewModel> _readMapper;

        public AbstractEntityCreateController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity> entityValidationService,
            ICreateViewModelMapper<TKey, TEntity, TCreateViewModel> mapper,
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readMapper)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _mapper = mapper;
            _readMapper = readMapper;
        }

        [HttpPost]
        [SwaggerOperation("Creates {entityNamePlural}")]
        [SwaggerResponse(201, "A {entityName} was created successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> Post(
             TCreateViewModel body,
            CancellationToken cancellationToken)
        {
            using (_createService)
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

                TEntity created;

                try
                {
                    created = await _createService
                        .CreateAsync(entity, cancellationToken)
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

                    return BadRequest(ModelState);
                }

                if (created == null)
                {
                    return BadRequest();
                }

                var createdViewModel = await _readMapper
                    .ToAsync(created, cancellationToken)
                    .ConfigureAwait(false);

                _createService = null;
                _mapper = null;
                _readMapper = null;
                _entityValidationService = null;

                return Created($"{Request.Path.Value}/{created.Id}", createdViewModel);
            }
        }
    }
}
