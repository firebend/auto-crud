using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityDeleteController<TKey, TEntity, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        private readonly IEntityDeleteService<TKey, TEntity> _deleteService;
        private readonly IReadViewModelMapper<TKey, TEntity, TViewModel> _viewModelMapper;

        protected AbstractEntityDeleteController(IEntityDeleteService<TKey, TEntity> deleteService,
            IEntityKeyParser<TKey, TEntity> entityKeyParser,
            IReadViewModelMapper<TKey, TEntity, TViewModel> viewModelMapper) : base(entityKeyParser)
        {
            _deleteService = deleteService;
            _viewModelMapper = viewModelMapper;
        }

        [HttpDelete("{id}")]
        [SwaggerOperation("Deletes {entityNamePlural}.")]
        [SwaggerResponse(200, "A {entityName} with the given key was deleted.")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [SwaggerResponse(400, "The request is invalid.")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TViewModel>> Delete(
            [Required][FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var key = GetKey(id);

            if (!key.HasValue)
            {
                return BadRequest(ModelState);
            }

            var deleted = await _deleteService
                .DeleteAsync(key.Value, cancellationToken)
                .ConfigureAwait(false);

            if (deleted == null)
            {
                return NotFound(new { id });
            }

            var mapped = await _viewModelMapper
                .ToAsync(deleted, cancellationToken)
                .ConfigureAwait(false);

            return Ok(mapped);
        }
    }
}
