using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Implementations.Defaults;

public class DefaultEntityOrderByProviderActive<TKey, TEntity> : DefaultDefaultEntityOrderByProvider<TKey, TEntity>
    where TEntity : IEntity<TKey>, IActiveEntity
    where TKey : struct
{
    public DefaultEntityOrderByProviderActive() : base(x => x.IsDeleted, false)
    {
    }
}
