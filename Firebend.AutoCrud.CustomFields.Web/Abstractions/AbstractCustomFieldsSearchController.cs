using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.CustomFields.Web.Abstractions
{
    public class AbstractCustomFieldsSearchController<TKey, TEntity> : ControllerBase
        where TKey : struct
    {
        private readonly ICustomFieldsSearchService<TKey> _searchService;

        public AbstractCustomFieldsSearchController(ICustomFieldsSearchService<TKey> searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("/custom-fields")]
        public async Task<ActionResult<EntityPagedResponse<CustomFieldsEntity<TKey>>>> GetAsync()
        {
            //TODO:
            return Ok(null);
        }
    }
}
