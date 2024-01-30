using Firebend.AutoCrud.Core.Interfaces.Models;
using Newtonsoft.Json.Linq;

namespace Firebend.AutoCrud.ChangeTracking.Models;

/// <summary>
/// Encapsulates change
/// </summary>
/// <typeparam name="TKey">
/// The type of key the entity uses.
/// </typeparam>
/// <typeparam name="TEntity">
/// The type of entity.
/// </typeparam>
public class ChangeTrackingEntity<TKey, TEntity> : ChangeTrackingModel<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public object DomainEventCustomContext { get; set; }

    public T GetDomainEventContext<T>()
        where T : class => DomainEventCustomContext switch
        {
            null => default,
            T context => context,
            JObject jObject => jObject.ToObject<T>(),
            _ => DomainEventCustomContext as T
        };
}
