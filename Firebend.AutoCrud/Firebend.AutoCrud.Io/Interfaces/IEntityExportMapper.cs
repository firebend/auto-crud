namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityExportMapper<in TEntity, out TOut>
        where TEntity: class
        where TOut: class
    {
        TOut Map(TEntity entity);
    }
}