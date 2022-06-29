using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityCreateController<TKey, TEntity, TCreateViewModel, TReadViewModel> : AbstractEntityControllerBase
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
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _mapper = mapper;
            _readMapper = readMapper;
        }

        [HttpPost]
        [SwaggerOperation("Creates {entityNamePlural}")]
        [SwaggerResponse(201, "A {entityName} was created successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> CreateAsync(
            TCreateViewModel body,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_createService);

            if (body == null)
            {
                ModelState.AddModelError("body", "A body is required");
                return GetInvalidModelStateResult();
            }

            var entity = await _mapper.FromAsync(body, cancellationToken)
                .ConfigureAwait(false);

            if (!TryValidateModel(entity))
            {
                return GetInvalidModelStateResult();
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

                return GetInvalidModelStateResult();
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

                return GetInvalidModelStateResult();
            }

            if (created == null)
            {
                return GetInvalidModelStateResult();
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
