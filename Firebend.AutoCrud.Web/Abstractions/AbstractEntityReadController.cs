using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityReadController<TKey, TEntity, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IReadViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;

        protected AbstractEntityReadController(IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IReadViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(entityKeyParser, apiOptions)
        {
            _readService = readService;
            _viewModelMapper = viewModelMapper;
        }

        [HttpGet("{id}")]
        [SwaggerOperation("Gets a specific {entityName}.")]
        [SwaggerResponse(200, "The {entityName} with the given key.")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TViewModel>> ReadByIdAsync(
            [Required][FromRoute] string id,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_readService);

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

            var mapped = await _viewModelMapper
                .ToAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }
    }
}
