using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Io.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Io.Web.Interfaces
{
    public interface IEntityExportControllerService<TKey, TEntity, TSearch, TMapped>
        where TSearch : EntitySearchRequest
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TMapped : class
    {
        Task<FileResult> ExportEntitiesAsync(EntityFileType fileType,
            string fileName,
            TSearch search,
            CancellationToken cancellationToken = default);
    }
}
