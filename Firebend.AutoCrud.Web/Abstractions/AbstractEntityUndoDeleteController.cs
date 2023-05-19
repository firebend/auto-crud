using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Web.Abstractions
{
    [ApiController]
    public abstract class AbstractEntityUndoDeleteController<TKey, TEntity, TVersion, TViewModel> : AbstractControllerWithKeyParser<TKey, TEntity, TVersion>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IActiveEntity
        where TVersion : class, IAutoCrudApiVersion
        where TViewModel : class
    {
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> _viewModelMapper;

        protected AbstractEntityUndoDeleteController(IEntityUpdateService<TKey, TEntity> updateService,
            IEntityKeyParser<TKey, TEntity, TVersion> entityKeyParser,
            IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> viewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions,
            IEntityReadService<TKey, TEntity> readService) : base(entityKeyParser, apiOptions)
        {
            _updateService = updateService;
            _viewModelMapper = viewModelMapper;
            _readService = readService;
        }

        [HttpPost("{id}/undo-delete")]
        [SwaggerOperation("Undoes a delete for a given {entityName}.")]
        [SwaggerResponse(200, "A {entityName} with the given key is not longer deleted.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        [SwaggerResponse(403, "Forbidden")]
        [SwaggerResponse(404, "The {entityName} with the given key is not found.")]
        [Produces("application/json")]
        public virtual async Task<ActionResult<TViewModel>> UndoDeleteAsync(
            [Required][FromRoute] string id,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_updateService);

            var key = GetKey(id);

            if (!key.HasValue)
            {
                return GetInvalidModelStateResult();
            }

            var entity = await _readService.GetByKeyAsync(key.Value, cancellationToken);

            if (entity is null)
            {
                return NotFound(new { id });
            }

            var patch = new JsonPatchDocument<TEntity>();
            patch.Replace(x => x.IsDeleted, false);

            var deleted = await _updateService.PatchAsync(key.Value, patch, cancellationToken);

            if (deleted == null)
            {
                return NotFound(new { id });
            }

            entity.IsDeleted = false;

            var mapped = await _viewModelMapper.ToAsync(entity, cancellationToken);

            return Ok(mapped);
        }
    }
}
