using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public class AbstractCustomFieldsSearchController<TKey, TEntity> : ControllerBase
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly ICustomFieldsSearchService<TKey, TEntity> _searchService;

        public AbstractCustomFieldsSearchController(ICustomFieldsSearchService<TKey, TEntity> searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("custom-fields")]
        [SwaggerOperation("Searches for custom fields assigned to a given {entityName}")]
        [SwaggerResponse(200, "All the custom fields for {entityNamePlural} that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.")]
        public async Task<ActionResult<EntityPagedResponse<CustomFieldsEntity<TKey>>>> GetAsync(
                [FromQuery]string key,
                [FromQuery]string value,
                [FromQuery]int pageNumber,
                [FromQuery]int pageSize,
                CancellationToken cancellationToken)
        {

            Response.RegisterForDispose(_searchService);

            if (pageNumber <= 0)
            {
                ModelState.AddModelError(nameof(pageNumber), "Must be greater than zero.");
                return BadRequest(ModelState);
            }

            if (pageSize <= 0)
            {
                ModelState.AddModelError(nameof(pageNumber), "Must be greater than zero.");
                return BadRequest(ModelState);
            }

            if (pageSize > 100)
            {
                ModelState.AddModelError(nameof(pageNumber), "Must be less than or equal to 100.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(key) && string.IsNullOrWhiteSpace(value))
            {
                ModelState.AddModelError(nameof(key), $"A {nameof(key)} or {nameof(value)} must be provided to search");
                return BadRequest(ModelState);
            }

            var result = await _searchService.SearchAsync(
                    key,
                    value,
                    pageNumber,
                    pageSize,
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
    }
}
