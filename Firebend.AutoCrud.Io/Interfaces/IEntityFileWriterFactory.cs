using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityFileWriterFactory
    {
        IEntityFileWriter Get(EntityFileType type);
    }
}
