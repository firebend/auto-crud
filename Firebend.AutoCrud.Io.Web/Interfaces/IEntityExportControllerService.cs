using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Io.Web.Interfaces
{
    public interface IEntityExportControllerService<in TSearch>
        where TSearch : EntitySearchRequest
    {
        Task<FileResult> ExportEntitiesAsync(EntityFileType fileType,
            string fileName,
            TSearch search,
            CancellationToken cancellationToken = default);
    }
}
