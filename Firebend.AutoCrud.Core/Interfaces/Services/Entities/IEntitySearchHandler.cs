using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntitySearchHandler<TKey, TEntity, in TSearch>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        IQueryable<TEntity> HandleSearch(IQueryable<TEntity> queryable, TSearch searchRequest);
    }
}
