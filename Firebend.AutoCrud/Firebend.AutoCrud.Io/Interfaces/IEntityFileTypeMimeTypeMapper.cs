using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileTypeMimeTypeMapper
    {
        string MapMimeType(EntityFileType entityFileType);

        string GetExtension(EntityFileType entityFileType);
    }
}