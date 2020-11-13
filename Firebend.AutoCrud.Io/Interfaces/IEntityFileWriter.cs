using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileWriter
    {
        EntityFileType FileType { get; }

        Task<Stream> WriteRecordsAsync<T>(IEnumerable<IFileFieldWrite<T>> fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default)
            where T : class;
    }

    public interface IEntityFileWriterCsv : IEntityFileWriter
    {
    }

    public interface IEntityFileWriterSpreadSheet : IEntityFileWriter
    {
    }
}
