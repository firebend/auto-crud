using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Abstractions
{
    public abstract class AbstractCsvHelperFileWriter : BaseDisposable,  IEntityFileWriter
    {
        public abstract EntityFileType FileType { get; }

        private IWriter _writer;
        private TextWriter _textWriter;
        private Stream _stream;

        public async Task<Stream> WriteRecordsAsync<T>(IEnumerable<IFileFieldWrite<T>> fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken = default)
            where T : class
        {
            _stream = AutoCrudPooledStream.GetStream("AutoCrudPooledStreamExport");

            var fieldArray = fields
                .OrderBy(x => x.FieldIndex)
                .ToArray();

            if (FileType == EntityFileType.Csv)
            {
                _textWriter = new StreamWriter(_stream);
            }

            _writer = FileType == EntityFileType.Csv ?
                new CsvWriter(_textWriter, GetCsvConfiguration())
                : new SpreadsheetWriter(_stream, "Export", GetCsvConfiguration());

            foreach (var fileField in fieldArray)
            {
                _writer.WriteField(fileField.FieldName);
            }

            await _writer.NextRecordAsync().ConfigureAwait(false);

            foreach (var record in records)
            {
                foreach (var field in fieldArray)
                {
                    var value = field.Writer(record);
                    _writer.WriteField(value);
                }

                await _writer.NextRecordAsync().ConfigureAwait(false);
            }

            if (_textWriter != null)
            {
                await _textWriter.FlushAsync().ConfigureAwait(false);
            }

            if (_writer is SpreadsheetWriter excelWriter)
            {
                excelWriter.SetWidths();
                excelWriter.SaveWorkbook();
            }

            _stream.Seek(0, SeekOrigin.Begin);

            return _stream;
        }

        private static CsvConfiguration GetCsvConfiguration() => new(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            LeaveOpen = true
        };

        protected override void DisposeManagedObjects()
        {
            _writer?.Dispose();
            _textWriter?.Dispose();
            _stream?.Dispose();
        }
    }
}
