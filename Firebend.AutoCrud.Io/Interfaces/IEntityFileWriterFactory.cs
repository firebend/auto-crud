using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Interfaces;

public interface IEntityFileWriterFactory<TVersion>
    where TVersion : class, IAutoCrudApiVersion
{
    public IEntityFileWriter<TVersion> Get(EntityFileType type);
}
