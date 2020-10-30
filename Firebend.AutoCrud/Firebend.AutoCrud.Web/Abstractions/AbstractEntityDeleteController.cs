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
    public abstract class AbstractEntityDeleteController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityDeleteService<TKey, TEntity> _deleteService;

        protected AbstractEntityDeleteController(IEntityDeleteService<TKey, TEntity> deleteService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser) : base(entityKeyParser)
        {
            _deleteService = deleteService;
        }

        [HttpDelete("{id}")]
        [SwaggerOperation("Deletes an entity.")]
        [SwaggerResponse(200, "An entity with the given key.")]
        [SwaggerResponse(404, "The entity with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Delete(
            [Required] [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var key = GetKey(id);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            var deleted = await _deleteService
                .DeleteAsync(key.Value, cancellationToken)
                .ConfigureAwait(false);

            if (deleted != null)
            {
                return Ok();
            }

            return NotFound(new { id });
        }
    }
}