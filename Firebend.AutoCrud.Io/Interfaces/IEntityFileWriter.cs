using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileWriter<TVersion> : IDisposable
        where TVersion : class, IAutoCrudApiVersion
    {
        EntityFileType FileType { get; }

        Task<Stream> WriteRecordsAsync<T>(IFileFieldWrite<T>[] fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken)
            where T : class;
    }

    public interface IEntityFileWriterCsv<TVersion> : IEntityFileWriter<TVersion>
        where TVersion : class, IAutoCrudApiVersion;

    public interface IEntityFileWriterSpreadSheet<TVersion> : IEntityFileWriter<TVersion>
        where TVersion : class, IAutoCrudApiVersion;
}
