using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkIncludesProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable);
}
