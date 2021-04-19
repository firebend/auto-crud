using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Abstractions
{
    public abstract class AbstractCsvHelperFileWriter : IEntityFileWriter
    {
        public abstract EntityFileType FileType { get; }

        public async Task<byte[]> WriteRecordsAsync<T>(IEnumerable<IFileFieldWrite<T>> fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var stream = AutoCrudPooledStream.GetStream("AutoCrudPooledStreamExport");

            var fieldArray = fields
                .OrderBy(x => x.FieldIndex)
                .ToArray();

            TextWriter textWriter = null;

            if (FileType == EntityFileType.Csv)
            {
                textWriter = new StreamWriter(stream);
            }

            IWriter writer = FileType == EntityFileType.Csv ?
                new CsvWriter(textWriter, GetCsvConfiguration())
                : new SpreadsheetWriter(stream, "Export", GetCsvConfiguration());

            foreach (var fileField in fieldArray)
            {
                writer.WriteField(fileField.FieldName);
            }

            await writer.NextRecordAsync().ConfigureAwait(false);

            foreach (var record in records)
            {
                foreach (var field in fieldArray)
                {
                    var value = field.Writer(record);
                    writer.WriteField(value);
                }

                await writer.NextRecordAsync().ConfigureAwait(false);
            }

            if (textWriter != null)
            {
                await textWriter.FlushAsync().ConfigureAwait(false);
            }

            if (writer is SpreadsheetWriter excelWriter)
            {
                excelWriter.SetWidths();
                excelWriter.SaveWorkbook();
            }

            stream.Seek(0, SeekOrigin.Begin);

            var bytes = stream.ToArray();

            await DisposeAll(writer, textWriter, stream);

            return bytes;
        }

        private static async Task DisposeAll(params IAsyncDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
                if (disposable == null)
                {
                    continue;
                }

                try
                {
                    await disposable.DisposeAsync();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static CsvConfiguration GetCsvConfiguration() => new(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            LeaveOpen = true
        };
    }
}
