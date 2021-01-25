using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityQueryCustomizer<TKey, TEntity, in TSearch>
        where TEntity: IEntity<TKey>
        where TKey : struct
        where TSearch : EntitySearchRequest
    {
        public T Customize<T>(T queryable,  TSearch searchRequest)
            where T : IQueryable<TEntity>;
    }
}
