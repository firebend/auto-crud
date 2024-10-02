using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;

public interface IChangeTrackingTableNameProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    (string Table, string Schema) GetTableName();
}
