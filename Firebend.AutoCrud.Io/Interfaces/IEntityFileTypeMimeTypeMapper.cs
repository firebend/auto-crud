using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IEntityFileTypeMimeTypeMapper<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public string MapMimeType(EntityFileType entityFileType);

    public string GetExtension(EntityFileType entityFileType);
}
