using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityCreateMultipleController<TKey, TEntity, TVersion, TMultipleViewModelWrapper, TMultipleViewModel, TReadViewModel>
        : AbstractEntityControllerBase<TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TMultipleViewModel : class
        where TReadViewModel : class
        where TMultipleViewModelWrapper : IMultipleEntityViewModel<TMultipleViewModel>
    {
        private readonly IEntityCreateService<TKey, TEntity> _createService;
        private readonly IEntityValidationService<TKey, TEntity, TVersion> _entityValidationService;
        private readonly ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TMultipleViewModelWrapper, TMultipleViewModel> _multipleMapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> _readMapper;

        protected AbstractEntityCreateMultipleController(IEntityCreateService<TKey, TEntity> createService,
            IEntityValidationService<TKey, TEntity, TVersion> entityValidationService,
            ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TMultipleViewModelWrapper, TMultipleViewModel> multipleMapper,
            IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> readMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _createService = createService;
            _entityValidationService = entityValidationService;
            _multipleMapper = multipleMapper;
            _readMapper = readMapper;
        }

        [HttpPost]
        [Route("multiple")]
        [SwaggerOperation("Creates multiple {entityNamePlural}")]
        [SwaggerResponse(200, "Multiple {entityNamePlural} were created successfully.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [SwaggerResponse(403, "Forbidden")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<CreateMultipleActionResult<TReadViewModel>>> CreateMultipleAsync(
            TMultipleViewModelWrapper body,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_createService);

            if (body?.Entities?.IsEmpty() ?? true)
            {
                ModelState.AddModelError("body", "A body is required");
                return GetInvalidModelStateResult();
            }

            var createdEntities = new List<TReadViewModel>();
            var errorEntities = new List<ModelStateResult<TReadViewModel>>();

            foreach (var toCreate in body.Entities)
            {
                var entityToCreate = await _multipleMapper
                    .FromAsync(body, toCreate, cancellationToken)
                    .ConfigureAwait(false);

                var isValid = await _entityValidationService
                    .ValidateAsync(null, entityToCreate, null, cancellationToken)
                    .ConfigureAwait(false);

                if (!isValid.WasSuccessful)
                {
                    var vm = await _readMapper.ToAsync(entityToCreate, cancellationToken);
                    var error = new ModelStateResult<TReadViewModel> { Message = isValid.Message };

                    foreach (var modelError in isValid.Errors)
                    {
                        error.AddError(modelError.PropertyPath, modelError.Error);
                    }

                    error.Model = vm;
                    errorEntities.Add(error);
                    continue;
                }

                if (isValid.Model != null)
                {
                    entityToCreate = isValid.Model;
                }

                if (entityToCreate == null)
                {
                    var result = new ModelStateResult<TReadViewModel>();
                    result.AddError("Entity", "The entity to create is null");
                    errorEntities.Add(result);
                    continue;

                }

                TEntity entity = null;

                try
                {
                    entity = await _createService
                        .CreateAsync(entityToCreate, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (AutoCrudEntityException ex)
                {
                    var modelStateResult = new ModelStateResult<TReadViewModel>();

                    if (ex.PropertyErrors != null)
                    {
                        foreach (var (property, error) in ex.PropertyErrors)
                        {
                            modelStateResult.AddError(property, error);
                        }
                    }

                    errorEntities.Add(modelStateResult);
                }

                if (entity == null)
                {
                    continue;
                }

                var mappedEntity = await _readMapper
                    .ToAsync(entity, cancellationToken)
                    .ConfigureAwait(false);

                createdEntities.Add(mappedEntity);
            }

            if (createdEntities.Count > 0)
            {
                return Ok(new CreateMultipleActionResult<TReadViewModel>
                {
                    Created = createdEntities,
                    Errors = errorEntities
                });
            }

            if (errorEntities.Count > 0)
            {
                return BadRequest(new CreateMultipleActionResult<TReadViewModel>
                {
                    Created = createdEntities,
                    Errors = errorEntities
                });
            }

            return BadRequest();
        }
    }
}
