using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public abstract class AbstractCustomFieldsSearchController<TKey, TEntity, TVersion> : AbstractEntityControllerBase<TVersion>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TVersion : class, IApiVersion
    {
        private readonly ICustomFieldsSearchService<TKey, TEntity> _searchService;
        private readonly IMaxPageSize<TKey, TEntity, TVersion> _maxPageSize;

        protected AbstractCustomFieldsSearchController(ICustomFieldsSearchService<TKey, TEntity> searchService,
            IMaxPageSize<TKey, TEntity, TVersion> maxPageSize,
            IOptions<ApiBehaviorOptions> apiOptions) : base(apiOptions)
        {
            _searchService = searchService;
            _maxPageSize = maxPageSize;
        }

        [HttpGet("custom-fields")]
        [SwaggerOperation("Searches for custom fields assigned to a given {entityName}")]
        [SwaggerResponse(200, "All the custom fields for {entityNamePlural} that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        public async Task<ActionResult<EntityPagedResponse<CustomFieldsEntity<TKey>>>> SearchCustomFieldsAsync(
            [FromQuery] CustomFieldsSearchRequest searchRequest,
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

            var result = await _searchService.SearchAsync(
                    searchRequest,
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
    }
}
