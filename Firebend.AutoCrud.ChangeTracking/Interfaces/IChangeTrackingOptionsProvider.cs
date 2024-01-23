using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.ChangeTracking.Interfaces;

public interface IChangeTrackingOptionsProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    ChangeTrackingOptions Options { get; }
}
