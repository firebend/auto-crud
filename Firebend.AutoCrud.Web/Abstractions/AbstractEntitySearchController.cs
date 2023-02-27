using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntitySearchController<TKey, TEntity, TVersion, TSearch, TViewModel> : AbstractEntityControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        where TSearch : IEntitySearchRequest
        where TViewModel : class
    {
        private readonly IEntitySearchService<TKey, TEntity, TSearch> _searchService;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> _viewModelMapper;
        private readonly IMaxPageSize<TKey, TEntity, TVersion> _maxPageSize;

        protected AbstractEntitySearchController(IEntitySearchService<TKey, TEntity, TSearch> searchService,
            IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> viewModelMapper,
            IMaxPageSize<TKey, TEntity, TVersion> maxPageSize,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _searchService = searchService;
            _viewModelMapper = viewModelMapper;
            _maxPageSize = maxPageSize;
        }

        [HttpGet]
        [SwaggerOperation("Searches for {entityNamePlural}")]
        [SwaggerResponse(200, "All the {entityNamePlural} that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        public virtual async Task<ActionResult<EntityPagedResponse<TViewModel>>> SearchAsync(
            [FromQuery] TSearch searchRequest,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_searchService);

            var validationResult = searchRequest.ValidateSearchRequest(_maxPageSize?.MaxPageSize);
            if (!validationResult.WasSuccessful)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyPath, error.Error);
                }
                return GetInvalidModelStateResult();
            }

            searchRequest.DoCount ??= true;

            var entities = await _searchService
                .PageAsync(searchRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!(entities?.Data?.Any() ?? false))
            {
                return Ok(entities);
            }

            var mapped = await _viewModelMapper
                .ToAsync(entities.Data, cancellationToken)
                .ConfigureAwait(false);

            var result = new EntityPagedResponse<TViewModel>
            {
                Data = mapped,
                CurrentPage = entities.CurrentPage,
                TotalRecords = entities.TotalRecords,
                CurrentPageSize = mapped.Count()
            };

            return Ok(result);
        }
    }
}
