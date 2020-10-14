using System;
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
    public abstract class AbstractEntityDeleteController<TEntity, TKey> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityDeleteService<TKey, TEntity> _deleteService;
        private readonly IEntityKeyParser<TKey, TEntity> _entityKeyParser;

        protected AbstractEntityDeleteController(IEntityDeleteService<TKey, TEntity> deleteService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser)
        {
            _deleteService = deleteService;
            _entityKeyParser = entityKeyParser;
        }

        [HttpDelete("{id}")]
        [SwaggerOperation("Deletes an entity.")]
        [SwaggerResponse(200, "An entity with the given key.")]
        [SwaggerResponse(404, "The entity with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> Delete([FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var key = _entityKeyParser.ParseKey(id);
            
            var deleted = await _deleteService
                .DeleteAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (deleted != null)
            {
                return Ok();
            }

            return NotFound(new { id });
        }
    }
}