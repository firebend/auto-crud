using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityDeleteController<TKey, TEntity, TVersion, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {
        private readonly IEntityDeleteService<TKey, TEntity> _deleteService;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> _viewModelMapper;

        protected AbstractEntityDeleteController(IEntityDeleteService<TKey, TEntity> deleteService,
            IEntityKeyParser<TKey, TEntity, TVersion> entityKeyParser,
            IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> viewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(entityKeyParser, apiOptions)
        {
            _deleteService = deleteService;
            _viewModelMapper = viewModelMapper;
        }

        [HttpDelete("{id}")]
        [SwaggerOperation("Deletes {entityNamePlural}.")]
        [SwaggerResponse(200, "A {entityName} with the given key was deleted.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TViewModel>> DeleteAsync(
            [Required][FromRoute] string id,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_deleteService);

            var key = GetKey(id);

            if (!key.HasValue)
            {
                return GetInvalidModelStateResult();
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
