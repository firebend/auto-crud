using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Including
{
    public class FunctionIncludesProvider<TKey, TEntity> : IEntityFrameworkIncludesProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _func;

        public FunctionIncludesProvider()
        {

        }

        public FunctionIncludesProvider(Func<IQueryable<TEntity>, IQueryable<TEntity>> func)
        {
            _func = func;
        }

        public IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable)
        {
            if (_func == null)
            {
                return queryable;
            }

            using var _ = AutoCrudDelegatePool.GetPooledFunction(_func, queryable, out var func);
            var query = func();
            return query;
        }
    }
}
