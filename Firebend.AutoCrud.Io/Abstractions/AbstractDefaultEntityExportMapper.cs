using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Abstractions;

public abstract class AbstractDefaultEntityExportMapper<T, TVersion> : IEntityExportMapper<T, TVersion, T>
    where T : class
    where TVersion : class, IAutoCrudApiVersion
{
    public T Map(T entity) => entity;
}
