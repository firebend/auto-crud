using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations;

public class EntityExportService<T, TVersion> : BaseDisposable, IEntityExportService<T, TVersion>
    where T : class
    where TVersion : class, IAutoCrudApiVersion
{
    private readonly IFileFieldAutoMapper<TVersion> _autoMapper;
    private readonly IEntityFileWriterFactory<TVersion> _fileWriterFactory;

    public EntityExportService(IEntityFileWriterFactory<TVersion> fileWriterFactory,
        IFileFieldAutoMapper<TVersion> autoMapper)
    {
        _fileWriterFactory = fileWriterFactory;
        _autoMapper = autoMapper;
    }

    public Task<Stream> ExportAsync(EntityFileType exportType,
        IEnumerable<T> records,
        CancellationToken cancellationToken)
    {
        var writer = _fileWriterFactory.Get(exportType) ?? throw new Exception($"Could not find writer for type {exportType}");

        using (writer)
        {
            var fields = _autoMapper.MapOutput<T>();

            return writer.WriteRecordsAsync(fields, records, cancellationToken);
        }
    }
}
