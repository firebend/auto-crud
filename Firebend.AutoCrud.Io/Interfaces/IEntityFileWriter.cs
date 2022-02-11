using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileWriter : IDisposable
    {
        EntityFileType FileType { get; }

        Task<Stream> WriteRecordsAsync<T>(IEnumerable<IFileFieldWrite<T>> fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken)
            where T : class;
    }

    public interface IEntityFileWriterCsv : IEntityFileWriter
    {
    }

    public interface IEntityFileWriterSpreadSheet : IEntityFileWriter
    {
    }
}
