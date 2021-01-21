using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Including
{
    public class DefaultEntityFrameworkIncludesProvider<TKey, TEntity> : IEntityFrameworkIncludesProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable) => null;
    }
}
