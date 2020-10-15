#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

#endregion

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityReadController<TKey, TEntity> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityKeyParser<TKey, TEntity> _entityKeyParser;
        private readonly IEntityReadService<TKey, TEntity> _readService;

        protected AbstractEntityReadController(IEntityReadService<TKey, TEntity> readService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser)
        {
            _readService = readService;
            _entityKeyParser = entityKeyParser;
        }

        [HttpGet("{id}")]
        [SwaggerOperation("Gets a specific entity")]
        [SwaggerResponse(200, "An entity with the given key.")]
        [SwaggerResponse(404, "The entity with the given key is not found.")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> GetById([FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var key = _entityKeyParser.ParseKey(id);

            var entity = await _readService
                .GetByKeyAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound(new {id});
            }

            return Ok(entity);
        }
    }
}