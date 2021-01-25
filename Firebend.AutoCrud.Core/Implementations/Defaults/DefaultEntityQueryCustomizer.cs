using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Core.Pooling;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityQueryCustomizer<TKey, TEntity, TSearch> : IEntityQueryCustomizer<TKey, TEntity, TSearch>
        where TSearch : EntitySearchRequest
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> _func;

        public DefaultEntityQueryCustomizer()
        {

        }

        public DefaultEntityQueryCustomizer(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
        {
            _func = func;
        }

        public T Customize<T>(T query, TSearch searchRequest)
            where T : IQueryable<TEntity>
        {
            if (_func == null)
            {
                return query;
            }

            using var _ = AutoCrudDelegatePool.GetPooledFunction(_func, searchRequest, out var pooledFunc);

            return (T)pooledFunc(query);
        }
    }
}
