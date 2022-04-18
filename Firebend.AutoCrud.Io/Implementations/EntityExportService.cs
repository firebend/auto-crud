using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityExportService<T> : BaseDisposable, IEntityExportService<T>
        where T : class
    {
        private readonly IFileFieldAutoMapper _autoMapper;
        private readonly IEntityFileWriterFactory _fileWriterFactory;

        public EntityExportService(IEntityFileWriterFactory fileWriterFactory,
            IFileFieldAutoMapper autoMapper)
        {
            _fileWriterFactory = fileWriterFactory;
            _autoMapper = autoMapper;
        }

        public Task<Stream> ExportAsync(EntityFileType exportType,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default)
        {
            var writer = _fileWriterFactory.Get(exportType);

            if (writer == null)
            {
                throw new Exception($"Could not find writer for type {exportType}");
            }

            using (writer)
            {
                var fields = _autoMapper.MapOutput<T>();

                return writer.WriteRecordsAsync(fields, records, cancellationToken);
            }
        }
    }
}
