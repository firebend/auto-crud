using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityReadAllController<TKey, TEntity, TViewModel> : ControllerBase, IAutoCrudController
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IReadViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;

        protected AbstractEntityReadAllController(IEntityReadService<TKey, TEntity> readService,
            IReadViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper)
        {
            _readService = readService;
            _viewModelMapper = viewModelMapper;
        }

        [HttpGet("all")]
        [SwaggerOperation("Gets all {entityNamePlural}")]
        [SwaggerResponse(200, "All the {entityNamePlural}.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        public virtual async Task<ActionResult<IEnumerable<TViewModel>>> GetAllAsync(CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_readService);

            var entities = await _readService
                .GetAllAsync(cancellationToken)
                .ConfigureAwait(false);

            if (entities == null || !entities.Any())
            {
                return Ok();
            }

            var mapped = await _viewModelMapper
                .ToAsync(entities, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }
    }
}
