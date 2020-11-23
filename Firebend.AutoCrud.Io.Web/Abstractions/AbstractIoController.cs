using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Io.Web.Abstractions
{
    public abstract class AbstractIoController<TKey, TEntity, TSearch, TMapped> : ControllerBase
        where TSearch : EntitySearchRequest
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TMapped : class
    {
        private readonly IEntityExportControllerService<TKey, TEntity, TSearch, TMapped> _exportService;
        private readonly IMaxExportPageSize<TKey, TEntity> _maxExportPageSize;

        protected AbstractIoController(IEntityExportControllerService<TKey, TEntity, TSearch, TMapped> exportService,
            IMaxExportPageSize<TKey, TEntity> maxExportPageSize = null)
        {
            _exportService = exportService;
            _maxExportPageSize = maxExportPageSize;
        }

        [HttpGet("export/{exportType}")]
        [SwaggerOperation("Exports {entityNamePlural} to a file.")]
        [SwaggerResponse(200, "A file with all the matched {entityNamePlural}.")]
        [SwaggerResponse(400, "The request is invalid.")]
        public virtual async Task<IActionResult> Search(
            [Required][FromRoute] string exportType,
            [Required][FromQuery] string filename,
            [FromQuery] TSearch searchRequest,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                ModelState.AddModelError(nameof(filename), $"{nameof(filename)} is invalid.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(exportType))
            {
                ModelState.AddModelError(nameof(exportType), $"{nameof(exportType)} is required.");
                return BadRequest(ModelState);
            }

            var entityExportType = exportType.ParseEnum<EntityFileType>();

            if (!entityExportType.HasValue || entityExportType.Value == EntityFileType.Unknown)
            {
                ModelState.AddModelError(nameof(exportType), $"{nameof(exportType)} is invalid");
                return BadRequest(ModelState);
            }

            if (searchRequest.PageNumber.HasValue && searchRequest.PageNumber <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"{nameof(searchRequest.PageNumber)} must be greater than zero.");
                return BadRequest(ModelState);
            }

            if (searchRequest.PageSize.HasValue && searchRequest.PageSize <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageSize), $"{nameof(searchRequest.PageSize)} must be greater than zero.");
                return BadRequest(ModelState);
            }

            if ((searchRequest.PageSize ?? 0) > 0 && (searchRequest.PageNumber ?? 0) <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"{nameof(searchRequest.PageNumber)} must be greater than zero.");
                return BadRequest(ModelState);
            }

            if ((_maxExportPageSize?.MaxPageSize ?? 0) > 0 && !searchRequest.PageSize.IsBetween(1, _maxExportPageSize.MaxPageSize))
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"Page size must be between 1 and {_maxExportPageSize.MaxPageSize}");

                return BadRequest(ModelState);
            }

            var fileResult = await _exportService.ExportEntitiesAsync(
                    entityExportType.GetValueOrDefault(),
                    filename,
                    searchRequest,
                    cancellationToken)
                .ConfigureAwait(false);

            return fileResult;
        }
    }
}
