using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Core.Pooling;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntitySearchHandler<TKey, TEntity, TSearch> : IEntitySearchHandler<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> _func;

        public DefaultEntitySearchHandler()
        {

        }

        public DefaultEntitySearchHandler(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
        {
            _func = func;
        }


        public IQueryable<TEntity> HandleSearch(IQueryable<TEntity> queryable, TSearch searchRequest)
        {
            if (_func == null)
            {
                return queryable;
            }

            using var _ = AutoCrudDelegatePool.GetPooledFunction(_func, searchRequest, out var pooled);
            return pooled(queryable);
        }
    }
}
