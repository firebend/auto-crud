using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IMaxPageSize<TKey, TEntity, TVersion>
    where TEntity : IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
{
    public int MaxPageSize { get; }
}
