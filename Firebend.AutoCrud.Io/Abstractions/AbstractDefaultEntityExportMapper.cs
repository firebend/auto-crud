using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Abstractions
{
    public abstract class AbstractDefaultEntityExportMapper<T> : IEntityExportMapper<T, T>
        where T : class
    {
        public T Map(T entity) => entity;
    }
}
