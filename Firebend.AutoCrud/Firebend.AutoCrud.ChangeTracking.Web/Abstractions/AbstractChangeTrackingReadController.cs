using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.ChangeTracking.Web.Abstractions
{
    public abstract class AbstractChangeTrackingReadController<TKey, TEntity> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IChangeTrackingReadService<TKey, TEntity> _read;

        protected AbstractChangeTrackingReadController(IChangeTrackingReadService<TKey, TEntity> read,
            IEntityKeyParser<TKey, TEntity> keyParser) : base(keyParser)
        {
            _read = read;
        }

        [HttpGet("{entityId}/changes")]
        [SwaggerOperation("Gets change tracking history for a specific entity")]
        [SwaggerResponse(200, "Change tracking history for the given entity key")]
        [Produces("application/json")]
        public virtual async Task<IActionResult> GetByEntityId(
            [Required] [FromRoute] string entityId,
            [Required] [FromQuery] EntitySearchRequest changeSearchRequest,
            CancellationToken cancellationToken)
        {
            var key = GetKey(entityId);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            if (changeSearchRequest == null)
            {
                ModelState.AddModelError(nameof(changeSearchRequest), "Search parameters are required.");

                return BadRequest(ModelState);
            }

            if (!changeSearchRequest.PageNumber.HasValue)
            {
                ModelState.AddModelError(nameof(changeSearchRequest.PageNumber), "Page number must have a value");

                return BadRequest(ModelState);
            }

            if (!changeSearchRequest.PageSize.GetValueOrDefault().IsBetween(1, 100))
            {
                ModelState.AddModelError(nameof(changeSearchRequest.PageNumber), "Page size must be between 1 and 100");

                return BadRequest(ModelState);
            }

            changeSearchRequest.DoCount ??= true;
            
            var changeRequest = new ChangeTrackingSearchRequest<TKey>
            {
                EntityId = key.Value,
                PageNumber = changeSearchRequest.PageNumber,
                PageSize = changeSearchRequest.PageSize
            };
            
            var changes = await _read
                .GetChangesByEntityId(changeRequest, cancellationToken)
                .ConfigureAwait(false);

            return Ok(changes);
        }
    }
}