#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

#endregion

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityReadAllController<TKey, TEntity> : ControllerBase
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityReadService<TKey, TEntity> _readService;

        protected AbstractEntityReadAllController(IEntityReadService<TKey, TEntity> readService)
        {
            _readService = readService;
        }

        [HttpGet("all")]
        [SwaggerOperation("Gets all entities")]
        [SwaggerResponse(200, "All the entities.")]
        [SwaggerResponse(400, "The request is invalid.")]
        public virtual async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _readService
                .GetAllAsync(cancellationToken)
                .ConfigureAwait(false);

            return Ok(entities);
        }
    }
}