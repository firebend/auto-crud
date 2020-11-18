using System;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Including
{
    public class FunctionIncludesProvider<TKey, TEntity> : IEntityFrameworkIncludesProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public FunctionIncludesProvider(Func<IQueryable<TEntity>, IQueryable<TEntity>> func)
        {
            Func = func;
        }

        public IQueryable<TEntity> AddIncludes(IQueryable<TEntity> queryable) => Func(queryable);

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Func { get; }
    }
}
