using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
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
    public abstract class AbstractEntitySearchController<TKey, TEntity, TSearch, TViewModel> : AbstractEntityControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
        where TViewModel : class
    {
        private readonly IEntitySearchService<TKey, TEntity, TSearch> _searchService;
        private readonly IReadViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;
        private readonly IMaxPageSize<TKey, TEntity> _maxPageSize;

        protected AbstractEntitySearchController(IEntitySearchService<TKey, TEntity, TSearch> searchService,
            IReadViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper,
            IMaxPageSize<TKey, TEntity> maxPageSize,
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
        public virtual async Task<ActionResult<EntityPagedResponse<TViewModel>>> SearchAsync(
            [FromQuery] TSearch searchRequest,
            CancellationToken cancellationToken)
        {
           Response.RegisterForDispose(_searchService);

            if (searchRequest == null)
            {
                ModelState.AddModelError(nameof(searchRequest), "Search parameters are required.");

                return GetInvalidModelStateResult();
            }

            if (!searchRequest.PageNumber.HasValue)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), "Page number must have a value");

                return GetInvalidModelStateResult();
            }

            var pageSize = _maxPageSize?.MaxPageSize ?? 100;

            if (!searchRequest.PageSize.GetValueOrDefault().IsBetween(1, pageSize))
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"Page size must be between 1 and {pageSize}");

                return GetInvalidModelStateResult();
            }

            if (searchRequest is IModifiedEntitySearchRequest modifiedEntitySearchRequest)
            {
                if (modifiedEntitySearchRequest.CreatedStartDate.HasValue && modifiedEntitySearchRequest.ModifiedStartDate.HasValue)
                {
                    if (modifiedEntitySearchRequest.CreatedStartDate.Value > modifiedEntitySearchRequest.ModifiedStartDate.Value)
                    {
                        ModelState.AddModelError(nameof(IModifiedEntitySearchRequest.CreatedStartDate), "Created date cannot be after modified date.");

                        return GetInvalidModelStateResult();
                    }
                }

                if (modifiedEntitySearchRequest.CreatedStartDate.HasValue && modifiedEntitySearchRequest.CreatedEndDate.HasValue)
                {
                    if (modifiedEntitySearchRequest.CreatedStartDate.Value > modifiedEntitySearchRequest.CreatedEndDate.Value)
                    {
                        ModelState.AddModelError(nameof(IModifiedEntitySearchRequest.CreatedStartDate), "Created start date must be before end date.");

                        return GetInvalidModelStateResult();
                    }
                }

                if (modifiedEntitySearchRequest.ModifiedEndDate.HasValue && modifiedEntitySearchRequest.ModifiedStartDate.HasValue)
                {
                    if (modifiedEntitySearchRequest.ModifiedStartDate.Value > modifiedEntitySearchRequest.ModifiedEndDate.Value)
                    {
                        ModelState.AddModelError(nameof(IModifiedEntitySearchRequest.ModifiedStartDate), "Modified start date must be before end date.");

                        return GetInvalidModelStateResult();
                    }
                }
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
                CurrentPageSize = entities.CurrentPageSize
            };

            return Ok(result);
        }
    }
}
