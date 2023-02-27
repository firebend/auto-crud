namespace Firebend.AutoCrud.Io.Interfaces
{
    public interface IEntityExportMapper<in TEntity, in TVersion, out TOut>
        where TEntity : class
        where TVersion : class
        where TOut : class
    {
        TOut Map(TEntity entity);
    }
}
