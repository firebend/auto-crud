using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.Paging
{
    public class DefaultMaxPageSize<TEntity, TKey> : IMaxExportPageSize<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public DefaultMaxPageSize(int max = 100)
        {
            MaxPageSize = max;
        }

        public int MaxPageSize { get; }
    }
}
