using System;
using System.Linq.Expressions;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityDefaultOrderByProvider<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        (Expression<Func<TEntity, object>>, bool ascending) GetOrderBy();
    }
}