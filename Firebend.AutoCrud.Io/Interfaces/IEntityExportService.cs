using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityExportService<in T, TVersion> : IDisposable
        where T : class
        where TVersion : class, IApiVersion
    {
        Task<Stream> ExportAsync(EntityFileType exportType,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default);
    }
}
