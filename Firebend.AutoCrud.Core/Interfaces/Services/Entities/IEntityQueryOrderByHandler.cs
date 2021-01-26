using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityQueryOrderByHandler<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public TQueryable OrderBy<TQueryable>(TQueryable queryable, IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys)
            where TQueryable : IQueryable<TEntity>;

    }
}
