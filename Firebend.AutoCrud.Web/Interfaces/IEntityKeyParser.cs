using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces
{
    // ReSharper disable once UnusedTypeParameter
    public interface IEntityKeyParser<TKey, TEntity, TVersion>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion
    {
        TKey? ParseKey(string key);
    }
}
