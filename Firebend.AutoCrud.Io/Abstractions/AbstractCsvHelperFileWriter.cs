
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.JsonPatch.Extensions;

namespace Firebend.AutoCrud.Io.Abstractions
{
    public abstract class AbstractCsvHelperFileWriter : BaseDisposable, IEntityFileWriter
    {
        private bool _disposed;

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
                _textWriter = new StreamWriter(_stream, null, -1, true);
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

            var childLists = typeof(T).GetProperties()
                .Where(propInfo => propInfo.PropertyType.IsCollection())
                .ToList();
            var hasAddedSubRows = false;

            using var recordEnumerator = records.GetEnumerator();

            while (recordEnumerator.MoveNext())
            {
                if (hasAddedSubRows)
                {
                    await _writer.NextRecordAsync().ConfigureAwait(false);
                    WriteHeader(fields);
                    await _writer.NextRecordAsync().ConfigureAwait(false);
                    hasAddedSubRows = false;
                }

                foreach (var fileFieldWrite in fields)
                {
                    _writer.WriteField(fileFieldWrite.Writer(recordEnumerator.Current));
                }

                await _writer.NextRecordAsync().ConfigureAwait(false);

                foreach (var propertyInfo in childLists)
                {
                    var itemType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    var listProperty = recordEnumerator.Current
                        .GetType()
                        .GetProperty(propertyInfo.Name)
                        .GetValue(recordEnumerator.Current);

                    if (listProperty != null)
                    {
                        var method = typeof(AbstractCsvHelperFileWriter)
                            .GetMethod(nameof(WriteSubRow), BindingFlags.NonPublic | BindingFlags.Instance);

                        dynamic dynamicResult = method.MakeGenericMethod(itemType).Invoke(this, new[]{ listProperty });
                        await dynamicResult;
                        hasAddedSubRows = dynamicResult.GetAwaiter().GetResult() || hasAddedSubRows;
                    }
                }
            }
        }

        private async Task<bool> WriteSubRow<T>(object listProperty)
            where T : class
        {
            var fields = FileFieldAutoMapper.MapOutput<T>();
            var records = listProperty as IEnumerable<T>;
            if (!records.IsEmpty())
            {
                await _writer.NextRecordAsync().ConfigureAwait(false);
                WriteHeader(fields);
                await WriteRows(fields, records);
                return true;
            }
            return false;
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
            LeaveOpen = true,
        };

        protected override void DisposeManagedObjects()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _writer?.Dispose();
            }
            catch
            {
                Console.WriteLine("Writer ex");
                // ignored
            }

            try
            {
                _textWriter?.Dispose();
            }
            catch
            {
                Console.WriteLine("Text Writer ex");
                // ignored
            }

            _disposed = true;
        }
    }
}
