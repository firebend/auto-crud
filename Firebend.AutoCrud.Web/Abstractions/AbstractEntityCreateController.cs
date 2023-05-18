using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityCreateController<TKey, TEntity, TVersion, TCreateViewModel, TReadViewModel> : AbstractEntityControllerBase<TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TCreateViewModel : class
        where TReadViewModel : class
    {
        private readonly IEntityCreateService<TKey, TEntity> _createService;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IEntityValidationService<TKey, TEntity, TVersion> _entityValidationService;
        private readonly ICreateViewModelMapper<TKey, TEntity, TVersion, TCreateViewModel> _mapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> _readMapper;

        public AbstractEntityCreateController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity, TVersion> entityValidationService,
            ICreateViewModelMapper<TKey, TEntity, TVersion, TCreateViewModel> mapper,
            IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> readMapper,
            IOptions<ApiBehaviorOptions> apiOptions,
            IEntityReadService<TKey, TEntity> readService) : base(apiOptions)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _mapper = mapper;
            _readMapper = readMapper;
            _readService = readService;
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

            var entity = await _mapper.FromAsync(body, cancellationToken);

            if (!TryValidateModel(entity))
            {
                return GetInvalidModelStateResult();
            }

            var isValid = await _entityValidationService.ValidateAsync(null, entity, null, cancellationToken);

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
                created = await _createService.CreateAsync(entity, cancellationToken);
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

            var read = await _readService.GetByKeyAsync(created.Id, cancellationToken);

            var createdViewModel = await _readMapper.ToAsync(read, cancellationToken);

            return Created($"{Request.Path.Value}/{created.Id}", createdViewModel);
        }
    }
}
