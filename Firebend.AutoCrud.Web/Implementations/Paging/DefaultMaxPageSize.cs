using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.Paging;

public class DefaultMaxPageSize<TEntity, TKey, TVersion> : IMaxExportPageSize<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TVersion : class, IAutoCrudApiVersion
{
    public DefaultMaxPageSize(int max = 100)
    {
        MaxPageSize = max;
    }

    public int MaxPageSize { get; }
}
