using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileTypeMimeTypeMapper<TVersion>
        where TVersion : class, IApiVersion
    {
        string MapMimeType(EntityFileType entityFileType);

        string GetExtension(EntityFileType entityFileType);
    }
}
