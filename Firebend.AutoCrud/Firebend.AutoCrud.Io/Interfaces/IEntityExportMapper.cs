namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityExportMapper<in TEntity, out TOut>
        where TEntity: class
        where TOut: class
    {
        TOut Map(TEntity entity);
    }

    public abstract class DefaultEntityExportMapper<T> : IEntityExportMapper<T, T> where T : class
    {
        public T Map(T entity)
        {
            return entity;
        }
    }
}