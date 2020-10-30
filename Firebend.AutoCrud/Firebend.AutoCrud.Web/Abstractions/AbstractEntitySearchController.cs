using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntitySearchController<TKey, TEntity, TSearch> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly IEntitySearchService<TKey, TEntity, TSearch> _searchService;

        protected AbstractEntitySearchController(IEntitySearchService<TKey, TEntity, TSearch> searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        [SwaggerOperation("Searches for entities")]
        [SwaggerResponse(200, "All the entities that match the search criteria.")]
        [SwaggerResponse(400, "The request is invalid.")]
        public virtual async Task<IActionResult> Search(
            [FromQuery] TSearch searchRequest,
            CancellationToken cancellationToken)
        {
            if (searchRequest == null)
            {
                ModelState.AddModelError(nameof(searchRequest), "Search parameters are required.");

                return BadRequest(ModelState);
            }

            if (!searchRequest.PageNumber.HasValue)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), "Page number must have a value");

                return BadRequest(ModelState);
            }

            if (!searchRequest.PageSize.GetValueOrDefault().IsBetween(1, 100))
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), "Page size must be between 1 and 100");

                return BadRequest(ModelState);
            }

            searchRequest.DoCount ??= true;

            var entities = await _searchService
                .PageAsync(searchRequest, cancellationToken)
                .ConfigureAwait(false);

            return Ok(entities);
        }
    }
}