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
    public abstract class AbstractEntityReadController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityReadService<TKey, TEntity> _readService;

        protected AbstractEntityReadController(IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser) : base(entityKeyParser)
        {
            _readService = readService;
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

            return Ok(entity);
        }
    }
}