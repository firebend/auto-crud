using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public class DefaultEntityOrderByProviderModified<TKey, TEntity> : DefaultDefaultEntityOrderByProvider<TKey, TEntity>
    where TEntity : IEntity<TKey>, IModifiedEntity
    where TKey : struct
{
    public DefaultEntityOrderByProviderModified() : base(x => x.ModifiedDate, false)
    {
    }
}
