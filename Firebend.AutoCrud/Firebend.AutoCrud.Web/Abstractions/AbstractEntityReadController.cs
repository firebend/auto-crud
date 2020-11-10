using System.ComponentModel.DataAnnotations;
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
    public abstract class AbstractEntityReadController<TKey, TEntity, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;
        private readonly IEntityReadService<TKey, TEntity> _readService;

        protected AbstractEntityReadController(IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper) : base(entityKeyParser)
        {
            _readService = readService;
            _viewModelMapper = viewModelMapper;
        }

        [HttpGet("{id}")]
        [SwaggerOperation("Gets a specific {entityName}.")]
        [SwaggerResponse(200, "The {entityName} with the given key.")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> GetById(
            [Required] [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var key = GetKey(id);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            var entity = await _readService
                .GetByKeyAsync(key.Value, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new {id});
            }

            var mapped = await _viewModelMapper
                .ToAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }
    }
}
