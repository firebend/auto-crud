using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.ChangeTracking.Web.Abstractions
{
    public abstract class AbstractChangeTrackingReadController<TKey, TEntity> : ControllerBase 
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IChangeTrackingReadService<TKey, TEntity> _read;
        private readonly IEntityKeyParser<TKey, TEntity> _keyParser;

        protected AbstractChangeTrackingReadController(IChangeTrackingReadService<TKey, TEntity> read,
            IEntityKeyParser<TKey, TEntity> keyParser)
        {
            _read = read;
            _keyParser = keyParser;
        }

        [HttpGet("{entityId}/changes")]
        [SwaggerOperation("Gets change tracking history for a specific entity")]
        [SwaggerResponse(200, "Change tracking history for the given entity key")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> GetByEntityId(
            [FromRoute][Required]string entityId,
            [FromQuery][Required]EntitySearchRequest page,
            CancellationToken cancellationToken)
        {
            var key = _keyParser.ParseKey(entityId);
            
            var changeRequest = new ChangeTrackingSearchRequest<TKey>
            {
                EntityId = key,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            };
            
            var changes = await _read
                .GetChangesByEntityId(changeRequest, cancellationToken)
                .ConfigureAwait(false);

            return Ok(changes);
        }
    }
}