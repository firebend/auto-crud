using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web.Interfaces;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.ChangeTracking.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractChangeTrackingReadController<TKey, TEntity, TVersion, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {
        private readonly IChangeTrackingReadService<TKey, TEntity> _read;
        private readonly IMaxPageSize<TKey, TEntity, TVersion> _maxPageSize;
        private readonly IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel> _mapper;

        protected AbstractChangeTrackingReadController(
            IEntityKeyParser<TKey, TEntity, TVersion> keyParser,
            IOptions<ApiBehaviorOptions> apiOptions,
            IChangeTrackingReadService<TKey, TEntity> read,
            IMaxPageSize<TKey, TEntity, TVersion> maxPageSize,
            IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel> mapper) : base(keyParser, apiOptions)
        {
            _read = read;
            _maxPageSize = maxPageSize;
            _mapper = mapper;
        }

        [HttpGet("{entityId}/changes")]
        [SwaggerOperation("Gets change tracking history for a specific {entityName}")]
        [SwaggerResponse(200, "Change tracking history for the given entity key")]
        [SwaggerResponse(403, "Forbidden")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<EntityPagedResponse<ChangeTrackingModel<TKey, TViewModel>>>> GetChangesAsync(
            [Required][FromRoute] string entityId,
            [Required][FromQuery] ModifiedEntitySearchRequest changeSearchRequest,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_read);

            var key = GetKey(entityId);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            var validationResult = changeSearchRequest.ValidateSearchRequest(_maxPageSize?.MaxPageSize);

            if (!validationResult.WasSuccessful)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyPath, error.Error);
                }

                return GetInvalidModelStateResult();
            }

            changeSearchRequest.DoCount ??= true;

            var changeRequest = new ChangeTrackingSearchRequest<TKey>();
            changeSearchRequest.CopyPropertiesTo(changeRequest);
            changeRequest.EntityId = key.Value;

            var changes = await _read.GetChangesByEntityId(changeRequest, cancellationToken);

            if (changes.Data.IsEmpty())
            {
                return Ok(changes);
            }

            var mapped = await _mapper.MapAsync(changes.Data, cancellationToken);

            return Ok(new EntityPagedResponse<ChangeTrackingModel<TKey, TViewModel>>
            {
                Data = mapped,
                CurrentPage = changes.CurrentPage,
                TotalRecords = changes.TotalRecords,
                CurrentPageSize = changes.CurrentPageSize
            });
        }
    }
}
