using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces
{
    public interface IEntityKeyParser<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        TKey ParseKey(string key);
    }
}