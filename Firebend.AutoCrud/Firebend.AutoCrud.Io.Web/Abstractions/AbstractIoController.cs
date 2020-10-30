using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Io.Web.Abstractions
{
    public abstract class AbstractIoController<TSearch>: ControllerBase
        where TSearch : EntitySearchRequest
    {
        private readonly IEntityExportControllerService<TSearch> _exportService;

        protected AbstractIoController(IEntityExportControllerService<TSearch> exportService)
        {
            _exportService = exportService;
        }


        [HttpGet("export/{exportType}")]
        [SwaggerOperation("Exports entities to a file.")]
        [SwaggerResponse(200, "A file with all the matched entities.")]
        [SwaggerResponse(400, "The request is invalid.")]
        public virtual async Task<IActionResult> Search(
            [Required] [FromRoute] string exportType,
            [Required] [FromQuery] string filename,
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