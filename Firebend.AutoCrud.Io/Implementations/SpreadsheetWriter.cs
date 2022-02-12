using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class SpreadsheetWriter : CsvWriter
    {
        private bool _disposed;
        private int _index = 1;
        private int _row = 1;

        protected bool LeaveOpen { get; }
        protected bool IsSanitizeForInjection { get; }
        protected Stream Stream { get; }

        protected IXLWorksheet Worksheet { get; }
        protected XLWorkbook Workbook { get; }

        public SpreadsheetWriter(Stream stream, string sheetName, CsvConfiguration configuration) : base(TextWriter.Null, configuration)
        {
            configuration.Validate();
            Workbook = new XLWorkbook(XLEventTracking.Disabled);
            Worksheet = GetOrAddWorksheet(Workbook, sheetName);
            Stream = stream;

            LeaveOpen = configuration.LeaveOpen;
            IsSanitizeForInjection = configuration.SanitizeForInjection;
        }

        private static IXLWorksheet GetOrAddWorksheet(IXLWorkbook workbook, string sheetName)
        {
            if (!workbook.TryGetWorksheet(sheetName, out var worksheet))
            {
                worksheet = workbook.AddWorksheet(sheetName);
            }

            return worksheet;
        }


        /// <inheritdoc />
        public override void WriteField(string field, bool shouldQuote)
        {
            if (IsSanitizeForInjection)
            {
                field = SanitizeForInjection(field);
            }

            WriteToCell(field);
            _index++;
        }

        /// <inheritdoc />
        public override void NextRecord()
        {
            Flush();
            _index = 1;
            _row++;
        }

        /// <inheritdoc />
        public override async Task NextRecordAsync()
        {
            await FlushAsync();
            _index = 1;
            _row++;
        }

        /// <inheritdoc />
        public override void Flush() => Stream.Flush();

        /// <inheritdoc />
        public override Task FlushAsync() => Stream.FlushAsync();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteToCell(string value)
        {
            var length = value?.Length ?? 0;

            if (value == null || length == 0)
            {
                return;
            }

            Worksheet.Worksheet.AsRange().Cell(_row, _index).Value = value;
        }

        public virtual void SetWidths()
        {
            Worksheet.Columns().AdjustToContents();
            Worksheet.Rows().AdjustToContents();
        }

        public virtual void SaveWorkbook() => Workbook.SaveAs(Stream);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (!LeaveOpen)
            {
                SaveWorkbook();
            }

            Stream.Flush();

            if (disposing)
            {
                // Dispose managed state (managed objects)
                Worksheet.Workbook.Dispose();

                if (!LeaveOpen)
                {
                    Stream.Dispose();
                }
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null

            _disposed = true;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            await FlushAsync().ConfigureAwait(false);
            Worksheet.Workbook.SaveAs(Stream);
            await Stream.FlushAsync().ConfigureAwait(false);

            if (disposing)
            {
                // Dispose managed state (managed objects)
                Worksheet.Workbook.Dispose();
                if (!LeaveOpen)
                {
                    await Stream.DisposeAsync().ConfigureAwait(false);
                }
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null


            _disposed = true;
        }
    }
}
