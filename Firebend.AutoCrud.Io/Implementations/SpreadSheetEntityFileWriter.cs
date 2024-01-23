using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Abstractions;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations;

public class SpreadSheetEntityFileWriter<TVersion> : AbstractCsvHelperFileWriter<TVersion>, IEntityFileWriterSpreadSheet<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public override EntityFileType FileType => EntityFileType.Spreadsheet;

    public SpreadSheetEntityFileWriter(IFileFieldAutoMapper<TVersion> autoMapper) : base(autoMapper)
    {
    }
}
