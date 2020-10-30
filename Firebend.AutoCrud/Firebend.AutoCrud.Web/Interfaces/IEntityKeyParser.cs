using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces
{
    // ReSharper disable once UnusedTypeParameter
    public interface IEntityKeyParser<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        TKey? ParseKey(string key);
    }
}