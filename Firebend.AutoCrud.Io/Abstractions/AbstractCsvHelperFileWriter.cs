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

            var (serializer, textWriter) = GetSerializer(stream);

            if (serializer == null)
            {
                throw new Exception($"Could not find serializer. Type: {FileType}");
            }

            var fieldArray = fields
                .OrderBy(x => x.FieldIndex)
                .ToArray();

            var csvWriter = new CsvWriter(serializer);

            foreach (var fileField in fieldArray)
            {
                csvWriter.WriteField(fileField.FieldName);
            }

            await csvWriter.NextRecordAsync().ConfigureAwait(false);

            foreach (var record in records)
            {
                foreach (var field in fieldArray)
                {
                    var value = field.Writer(record);
                    csvWriter.WriteField(value);
                }

                await csvWriter.NextRecordAsync().ConfigureAwait(false);
            }

            if (textWriter != null)
            {
                await textWriter.FlushAsync().ConfigureAwait(false);
            }

            if (serializer is SpreadsheetSerializer spreadsheetSerializer)
            {
                spreadsheetSerializer.SetWidths();
                spreadsheetSerializer.SaveWorkbook();
            }

            stream.Seek(0, SeekOrigin.Begin);

            var bytes = stream.ToArray();

            await DisposeAll(csvWriter, serializer, textWriter, stream);

            return bytes;
        }

        private static async Task DisposeAll(params IAsyncDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
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

        private static CsvConfiguration GetCsvConfiguration() => new (CultureInfo.InvariantCulture) { IgnoreBlankLines = true };

        private (ISerializer serializer, TextWriter writer) GetSerializer(Stream stream)
        {
            switch (FileType)
            {
                case EntityFileType.Csv:
                {
                    var writer = new StreamWriter(stream);
                    var serializer = new CsvSerializer(writer, GetCsvConfiguration());

                    return (serializer, writer);
                }
                case EntityFileType.Spreadsheet:
                    return (new SpreadsheetSerializer(stream, "Export", false, GetCsvConfiguration()), null);
                case EntityFileType.Unknown:
                    return (null, null);
                default:
                    return (null, null);
            }
        }
    }
}
