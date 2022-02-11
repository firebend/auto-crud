using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public abstract class AbstractCsvHelperFileWriter : BaseDisposable, IEntityFileWriter
    {
        public abstract EntityFileType FileType { get; }

        private IWriter _writer;
        private TextWriter _textWriter;
        private Stream _stream;

        public async Task<Stream> WriteRecordsAsync<T>(IFileFieldWrite<T>[] fields,
            IEnumerable<T> records,
            CancellationToken cancellationToken)
            where T : class
        {
            _stream = new MemoryStream();

            if (FileType == EntityFileType.Csv)
            {
                _textWriter = new StreamWriter(_stream);
            }

            _writer = FileType == EntityFileType.Csv
                ? new CsvWriter(_textWriter, GetCsvConfiguration())
                : new SpreadsheetWriter(_stream, "Export", GetCsvConfiguration());

            WriteHeader(fields);

            await WriteRows(fields, records);

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

        private async Task WriteRows<T>(IFileFieldWrite<T>[] fields, IEnumerable<T> records)
            where T : class
        {
            await _writer.NextRecordAsync().ConfigureAwait(false);

            using var recordEnumerator = records.GetEnumerator();

            while (recordEnumerator.MoveNext())
            {
                foreach (var fileFieldWrite in fields)
                {
                    _writer.WriteField(fileFieldWrite.Writer(recordEnumerator.Current));
                }

                await _writer.NextRecordAsync().ConfigureAwait(false);
            }
        }

        private void WriteHeader<T>(IFileFieldWrite<T>[] fields)
            where T : class
        {
            foreach (var t in fields)
            {
                _writer.WriteField(t.FieldName);
            }
        }

        private static CsvConfiguration GetCsvConfiguration() => new(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            LeaveOpen = true
        };

        protected override void DisposeManagedObjects()
        {
            try
            {
                _writer?.Dispose();
            }
            catch
            {
                // ignored
            }

            try
            {
                _textWriter?.Dispose();
            }
            catch
            {
                // ignored
            }

            try
            {
                _stream?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}
