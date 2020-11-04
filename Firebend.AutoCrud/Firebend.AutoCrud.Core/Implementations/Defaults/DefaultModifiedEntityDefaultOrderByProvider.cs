using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultModifiedEntityDefaultOrderByProvider<TKey, TEntity>: DefaultEntityDefaultOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>, IModifiedEntity
        where TKey : struct
    {
        public DefaultModifiedEntityDefaultOrderByProvider()
        {
            OrderBy = (x => x.ModifiedDate, false);
        }
    }
}