using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Abstractions;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations;

public class CsvEntityFileWriter<TVersion> : AbstractCsvHelperFileWriter<TVersion>, IEntityFileWriterCsv<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public override EntityFileType FileType => EntityFileType.Csv;

    public CsvEntityFileWriter(IFileFieldAutoMapper<TVersion> autoMapper) : base(autoMapper)
    {
    }
}
