using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class SpreadsheetSerializer : ISerializer
    {
        private readonly bool _disposeWorkbook;
        private readonly Stream _stream;
        private int _currentRow = 1;
        private bool _disposed;

        public SpreadsheetSerializer(Stream stream,
            string sheetName = "Export",
            bool dispose = true,
            CsvConfiguration configuration = null)
        {
            _stream = stream;
            _disposeWorkbook = dispose;

            Configuration = configuration ?? new CsvConfiguration(CultureInfo.InvariantCulture);
            Configuration.ShouldQuote = (s, context) => false;
            Context = new WritingContext(TextWriter.Null, Configuration, false);

            Workbook = new XLWorkbook();

            Worksheet = GetOrAddWorksheet(Workbook, sheetName);
        }

        public CsvConfiguration Configuration { get; }

        public XLWorkbook Workbook { get; }

        public IXLWorksheet Worksheet { get; }

        public int RowOffset { get; } = 0;

        public int ColumnOffset { get; } = 0;

        public virtual void Write(string[] record)
        {
            CheckDisposed();

            for (var i = 0; i < record.Length; i++)
            {
                Worksheet
                    .AsRange()
                    .Cell(_currentRow + RowOffset, i + 1 + ColumnOffset)
                    .Value = ReplaceHexadecimalSymbols(record[i]);
            }

            _currentRow++;
        }

        public Task WriteAsync(string[] record)
        {
            Write(record);

            return Task.CompletedTask;
        }

        public void WriteLine()
        {
        }

        public Task WriteLineAsync()
        {
            WriteLine();

            return Task.CompletedTask;
        }

        public WritingContext Context { get; }

        ISerializerConfiguration ISerializer.Configuration => Configuration;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                Dispose();

                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }

        private static string ReplaceHexadecimalSymbols(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return Regex.Replace(text, "[\x00-\x08\x0B\x0C\x0E-\x1F]", string.Empty, RegexOptions.Compiled);
        }

        ~SpreadsheetSerializer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SaveWorkbook();

                if (_disposeWorkbook)
                {
                    _stream.Close();
                    _stream.Dispose();
                }
            }

            _disposed = true;
        }

        public virtual void SaveWorkbook() => Workbook.SaveAs(_stream);

        public virtual void SetWidths()
        {
            Worksheet.Columns().AdjustToContents();
            Worksheet.Rows().AdjustToContents();
        }

        protected virtual void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }
        }

        private static IXLWorksheet GetOrAddWorksheet(IXLWorkbook workbook, string sheetName)
        {
            if (!workbook.TryGetWorksheet(sheetName, out var worksheet))
            {
                worksheet = workbook.AddWorksheet(sheetName);
            }

            return worksheet;
        }
    }
}
