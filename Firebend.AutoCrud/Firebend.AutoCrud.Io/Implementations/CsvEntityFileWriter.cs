using Firebend.AutoCrud.Io.Abstractions;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class CsvEntityFileWriter : AbstractCsvHelperFileWriter, IEntityFileWriterCsv
    {
        public override EntityFileType FileType { get; } = EntityFileType.Csv;
    }
}