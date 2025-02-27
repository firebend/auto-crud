namespace Firebend.AutoCrud.Io.Interfaces;

public interface IEntityExportMapper<in TEntity, TVersion, out TOut>
    where TEntity : class
    where TVersion : class
    where TOut : class
{
    public TOut Map(TEntity entity);
}
