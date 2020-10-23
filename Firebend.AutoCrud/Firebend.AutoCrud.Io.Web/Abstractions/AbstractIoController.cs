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
            [FromQuery] TSearch searchRequest,
            [Required] [FromQuery] string filename,
            CancellationToken cancellationToken)
        {
            var entityExportType = exportType.ParseEnum<EntityFileType>();

            if (!entityExportType.HasValue)
            {
                return BadRequest($"The provided export type is invalid");
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