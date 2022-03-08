using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public abstract class AbstractCustomFieldsSearchController<TKey, TEntity> : AbstractEntityControllerBase
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly ICustomFieldsSearchService<TKey, TEntity> _searchService;

        protected AbstractCustomFieldsSearchController(ICustomFieldsSearchService<TKey, TEntity> searchService,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _searchService = searchService;
        }

        [HttpGet("custom-fields")]
        [SwaggerOperation("Searches for custom fields assigned to a given {entityName}")]
        [SwaggerResponse(200, "All the custom fields for {entityNamePlural} that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        public async Task<ActionResult<EntityPagedResponse<CustomFieldsEntity<TKey>>>> SearchCustomFieldsAsync(
            [FromQuery] CustomFieldsSearchRequest searchRequest,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_searchService);

            if (searchRequest.PageNumber <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), "Must be greater than zero.");
                return GetInvalidModelStateResult();
            }

            if (searchRequest.PageSize <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageSize), "Must be greater than zero.");
                return GetInvalidModelStateResult();
            }

            if (searchRequest.PageSize > 100)
            {
                ModelState.AddModelError(nameof(searchRequest.PageSize), "Must be less than or equal to 100.");
                return GetInvalidModelStateResult();
            }

            if (string.IsNullOrWhiteSpace(searchRequest.Key) && string.IsNullOrWhiteSpace(searchRequest.Value))
            {
                ModelState.AddModelError(nameof(searchRequest.Key),
                    $"A {nameof(searchRequest.Key)} or {nameof(searchRequest.Value)} must be provided to search");
                return GetInvalidModelStateResult();
            }

            var result = await _searchService.SearchAsync(
                    searchRequest,
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
    }
}
