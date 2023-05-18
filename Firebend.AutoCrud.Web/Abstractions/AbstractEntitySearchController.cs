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
    public abstract class AbstractEntitySearchController<TKey, TEntity, TVersion, TSearch, TSearchViewModel, TReadViewModel>
        : AbstractEntityControllerBase<TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TSearch : IEntitySearchRequest
        where TReadViewModel : class
        where TSearchViewModel : class
    {
        private readonly IEntitySearchService<TKey, TEntity, TSearch> _searchService;

        private readonly ISearchViewModelMapper<TKey, TEntity, TVersion, TSearchViewModel, TSearch>
            _searchViewModelMapper;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> _readViewModelMapper;
        private readonly IMaxPageSize<TKey, TEntity, TVersion> _maxPageSize;

        protected AbstractEntitySearchController(IEntitySearchService<TKey, TEntity, TSearch> searchService,
            ISearchViewModelMapper<TKey, TEntity, TVersion, TSearchViewModel, TSearch> searchViewModelMapper,
            IReadViewModelMapper<TKey, TEntity, TVersion, TReadViewModel> readViewModelMapper,
            IMaxPageSize<TKey, TEntity, TVersion> maxPageSize,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _searchService = searchService;
            _searchViewModelMapper = searchViewModelMapper;
            _readViewModelMapper = readViewModelMapper;
            _maxPageSize = maxPageSize;
        }

        [HttpGet]
        [SwaggerOperation("Searches for {entityNamePlural}")]
        [SwaggerResponse(200, "All the {entityNamePlural} that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        public virtual async Task<ActionResult<EntityPagedResponse<TReadViewModel>>> SearchAsync(
            [FromQuery] TSearchViewModel searchViewModel,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_searchService);

            var searchRequest = await _searchViewModelMapper.FromAsync(searchViewModel, cancellationToken);

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

            var entities = await _searchService.PageAsync(searchRequest, cancellationToken);

            if (!(entities?.Data?.Any() ?? false))
            {
                return Ok(entities);
            }

            var mapped = await _readViewModelMapper.ToAsync(entities.Data, cancellationToken);

            var result = new EntityPagedResponse<TReadViewModel>
            {
                // ReSharper disable PossibleMultipleEnumeration
                Data = mapped,
                CurrentPageSize = mapped.Count(),
                // ReSharper restore PossibleMultipleEnumeration
                CurrentPage = entities.CurrentPage,
                TotalRecords = entities.TotalRecords,
            };


            return Ok(result);
        }
    }
}
