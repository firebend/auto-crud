using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;

public record TableNameResult(string Table, string Schema);

public interface IChangeTrackingTableNameProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    TableNameResult GetTableName();
}
