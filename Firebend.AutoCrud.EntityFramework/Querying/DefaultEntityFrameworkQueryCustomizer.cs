using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Querying
{
    public class DefaultEntityFrameworkQueryCustomizer<TKey, TEntity, TSearch> : IEntityFrameworkQueryableCustomizer<TKey, TEntity, TSearch>
        where TSearch : EntitySearchRequest
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> _func;

        public DefaultEntityFrameworkQueryCustomizer()
        {

        }

        public DefaultEntityFrameworkQueryCustomizer(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
        {
            _func = func;
        }

        public IQueryable<TEntity> Customize(IQueryable<TEntity> query, TSearch searchRequest)
        {
            if (_func == null)
            {
                return query;
            }

            using var _ = AutoCrudDelegatePool.GetPooledFunction(_func, searchRequest, out var pooledFunc);

            return pooledFunc(query);
        }
    }
}
