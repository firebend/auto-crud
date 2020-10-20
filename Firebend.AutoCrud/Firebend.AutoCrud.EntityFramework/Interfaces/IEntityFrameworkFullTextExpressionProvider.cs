using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>
        where TKey: struct
        where TEntity : IEntity<TKey>
    {
        public Expression<Func<TEntity, string, bool>> Filter { get; }
    }
}