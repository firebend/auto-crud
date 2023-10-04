using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityQueryOrderByHandler<TKey, TEntity> : IEntityQueryOrderByHandler<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IDefaultEntityOrderByProvider<TKey, TEntity> _defaultEntityOrderByProvider;

        public DefaultEntityQueryOrderByHandler(IDefaultEntityOrderByProvider<TKey, TEntity> defaultEntityOrderByProvider = null)
        {
            _defaultEntityOrderByProvider = defaultEntityOrderByProvider;
        }

        public TQueryable OrderBy<TQueryable>(TQueryable queryable, IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> orderBys)
            where TQueryable : IQueryable<TEntity>
        {
            IEnumerable<(Expression<Func<TEntity, object>> order, bool ascending)> order;

            var orderByArrays = orderBys as (Expression<Func<TEntity, object>> order, bool ascending)[] ?? orderBys?.ToArray();

            if (orderByArrays?.HasValues() ?? false)
            {
                order = orderByArrays;
            }
            else
            {
                if (_defaultEntityOrderByProvider != null)
                {
                    order = new[]
                    {
                        _defaultEntityOrderByProvider.GetOrderBy()
                    };
                }
                else
                {
                    order = Enumerable.Empty<(Expression<Func<TEntity, object>> order, bool ascending)>();
                }
            }

            IOrderedQueryable<TEntity> ordered = null;

            foreach (var (orderExpression, ascending) in order.Where(orderBy => orderBy != default))
            {
                if (ordered == null)
                {
                    ordered = ascending ? queryable.OrderBy(orderExpression) : queryable.OrderByDescending(orderExpression);
                }
                else
                {
                    ordered = ascending ? ordered.ThenBy(orderExpression) : ordered.ThenByDescending(orderExpression);
                }
            }

            if (ordered != null)
            {
                queryable = (TQueryable)ordered;
            }

            return queryable;
        }
    }
}
