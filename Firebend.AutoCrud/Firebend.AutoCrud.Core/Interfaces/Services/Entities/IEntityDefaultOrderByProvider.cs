#region

using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityDefaultOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        (Expression<Func<TEntity, object>>, bool ascending) GetOrderBy();
    }
}