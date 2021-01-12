using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public abstract class AbstractChangeTrackingReadController<TKey, TEntity, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IChangeTrackingReadService<TKey, TEntity> _read;
        private readonly IReadViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;
        private readonly IMaxPageSize<TKey, TEntity> _maxPageSize;

        protected AbstractChangeTrackingReadController(IChangeTrackingReadService<TKey, TEntity> read,
            IEntityKeyParser<TKey, TEntity> keyParser,
            IReadViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper,
            IMaxPageSize<TKey, TEntity> maxPageSize) : base(keyParser)
        {
            _read = read;
            _viewModelMapper = viewModelMapper;
            _maxPageSize = maxPageSize;
        }

        [HttpGet("{entityId}/changes")]
        [SwaggerOperation("Gets change tracking history for a specific {entityName}")]
        [SwaggerResponse(200, "Change tracking history for the given entity key")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<EntityPagedResponse<ChangeTrackingViewModel<TKey, TEntity, TViewModel>>>> GetByEntityId(
            [Required][FromRoute] string entityId,
            [Required][FromQuery] EntitySearchRequest changeSearchRequest,
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

            var maxPageSize = _maxPageSize?.MaxPageSize ?? 100;

            if (!changeSearchRequest.PageSize.GetValueOrDefault().IsBetween(1, maxPageSize))
            {
                ModelState.AddModelError(nameof(changeSearchRequest.PageNumber), $"Page size must be between 1 and {maxPageSize}");

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

            if (!(changes?.Data?.Any() ?? false))
            {
                return Ok(changes);
            }

            var tasks = changes
                .Data
                .Select(x => new ChangeTrackingViewModel<TKey, TEntity, TViewModel>().MapAsync(x, _viewModelMapper, cancellationToken))
                .ToArray();

            await Task.WhenAll(tasks);

            return Ok(new EntityPagedResponse<ChangeTrackingViewModel<TKey, TEntity, TViewModel>>
            {
                Data = tasks.Select(x => x.Result),
                CurrentPage = changes.CurrentPage,
                TotalRecords = changes.TotalRecords,
                CurrentPageSize = changes.CurrentPageSize
            });
        }
    }
}
