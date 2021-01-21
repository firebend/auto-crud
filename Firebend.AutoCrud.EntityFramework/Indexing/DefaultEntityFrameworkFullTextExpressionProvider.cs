using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Indexing
{
    public class DefaultEntityFrameworkFullTextExpressionProvider<TKey, TEntity> : IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public DefaultEntityFrameworkFullTextExpressionProvider()
        {
        }

        public DefaultEntityFrameworkFullTextExpressionProvider(Expression<Func<TEntity, string, bool>> filter)
        {
            Filter = filter;
        }

        public virtual Expression<Func<TEntity, string, bool>> Filter { get; }
    }
}
