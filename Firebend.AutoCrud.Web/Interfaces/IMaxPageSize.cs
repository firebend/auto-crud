using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces
{
    public interface IMaxPageSize<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public int MaxPageSize { get; }
    }

    public interface IMaxExportPageSize<TKey, TEntity> : IMaxPageSize<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {

    }
}
