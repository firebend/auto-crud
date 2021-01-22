using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkQueryableCustomizer<TKey, TEntity, in TSearch>
        where TEntity: IEntity<TKey>
        where TKey : struct
        where TSearch : EntitySearchRequest
    {
        public IQueryable<TEntity> Customize(IQueryable<TEntity> queryable,  TSearch searchRequest);
    }
}
