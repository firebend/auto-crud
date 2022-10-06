using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityValidateCreateController<TKey, TEntity, TCreateViewModel, TReadViewModel> : AbstractEntityControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TCreateViewModel : class
        where TReadViewModel : class
    {
        private ICreateViewModelMapper<TKey, TEntity, TCreateViewModel> _mapper;
        private IReadViewModelMapper<TKey, TEntity, TReadViewModel> _readMapper;

        public AbstractEntityValidateCreateController(ICreateViewModelMapper<TKey, TEntity, TCreateViewModel> mapper,
            IReadViewModelMapper<TKey, TEntity, TReadViewModel> readMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _mapper = mapper;
            _readMapper = readMapper;
        }

        [HttpPost("validate")]
        [SwaggerOperation("Shallow Validation on {entityNamePlural}")]
        [SwaggerResponse(201, "A {entityName} was validated successfully.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TReadViewModel>> CreateAsync(
            TCreateViewModel body,
            CancellationToken cancellationToken)
        {

            var entity = await this.ValidateModel(body, _mapper, cancellationToken);

            if (!ModelState.IsValid)
            {
                return GetInvalidModelStateResult();
            }

            var createdViewModel = await _readMapper
                .ToAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            _mapper = null;
            _readMapper = null;

            return Created($"{Request.Path.Value}/{Guid.Empty}", createdViewModel);
        }
    }
}
