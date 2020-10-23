using Firebend.AutoCrud.Io.Abstractions;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class SpreadSheetEntityFileWriter : AbstractCsvHelperFileWriter, IEntityFileWriterSpreadSheet
    {
        public override EntityFileType FileType { get; } = EntityFileType.Spreadsheet;
    }
}