using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Firebend.AutoCrud.Io.Web.Abstractions
{
    public abstract class AbstractIoController<TKey, TEntity, TSearch, TMapped> : AbstractEntityControllerBase
        where TSearch : EntitySearchRequest
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TMapped : class
    {
        private readonly IEntityExportControllerService<TKey, TEntity, TSearch, TMapped> _exportService;
        private readonly IMaxExportPageSize<TKey, TEntity> _maxExportPageSize;

        protected AbstractIoController(IEntityExportControllerService<TKey, TEntity, TSearch, TMapped> exportService,
            IOptions<ApiBehaviorOptions> apiOptions,
            IMaxExportPageSize<TKey, TEntity> maxExportPageSize = null) : base(apiOptions)
        {
            _exportService = exportService;
            _maxExportPageSize = maxExportPageSize;
        }

        [HttpGet("export/{exportType}")]
        [SwaggerOperation("Exports {entityNamePlural} to a file.")]
        [SwaggerResponse(200, "A file with all the matched {entityNamePlural}.")]
        [SwaggerResponse(400, "The request is invalid.", typeof(ValidationProblemDetails))]
        public virtual async Task<IActionResult> ExportAsync(
            [Required][FromRoute] string exportType,
            [Required][FromQuery] string filename,
            [FromQuery] TSearch searchRequest,
            CancellationToken cancellationToken)
        {
            Response.RegisterForDispose(_exportService);

            if (string.IsNullOrWhiteSpace(filename))
            {
                ModelState.AddModelError(nameof(filename), $"{nameof(filename)} is invalid.");
                return GetInvalidModelStateResult();
            }

            if (string.IsNullOrWhiteSpace(exportType))
            {
                ModelState.AddModelError(nameof(exportType), $"{nameof(exportType)} is required.");
                return GetInvalidModelStateResult();
            }

            if (searchRequest.PageNumber is <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"{nameof(searchRequest.PageNumber)} must be greater than zero.");
                return GetInvalidModelStateResult();
            }

            if (searchRequest.PageSize is <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageSize), $"{nameof(searchRequest.PageSize)} must be greater than zero.");
                return GetInvalidModelStateResult();
            }

            if ((searchRequest.PageSize ?? 0) > 0 && (searchRequest.PageNumber ?? 0) <= 0)
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"{nameof(searchRequest.PageNumber)} must be greater than zero.");
                return GetInvalidModelStateResult();
            }

            if ((_maxExportPageSize?.MaxPageSize ?? 0) > 0 && !searchRequest.PageSize.IsBetween(1, _maxExportPageSize.MaxPageSize))
            {
                ModelState.AddModelError(nameof(searchRequest.PageNumber), $"Page size must be between 1 and {_maxExportPageSize.MaxPageSize}");
                return GetInvalidModelStateResult();
            }

            var entityExportType = exportType.ParseEnum<EntityFileType>();

            if (entityExportType is null or EntityFileType.Unknown)
            {
                ModelState.AddModelError(nameof(exportType), $"{nameof(exportType)} is invalid");
                return GetInvalidModelStateResult();
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
