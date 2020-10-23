using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityExportService<in T>
        where T : class
    {
        Task<Stream> ExportAsync(EntityFileType exportType,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default);
    }
}