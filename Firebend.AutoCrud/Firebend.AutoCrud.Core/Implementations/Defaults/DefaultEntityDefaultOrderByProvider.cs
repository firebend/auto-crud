#region

using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

#endregion

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityDefaultOrderByProvider<TKey, TEntity> : IEntityDefaultOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public (Expression<Func<TEntity, object>>, bool ascending) GetOrderBy()
        {
            return default;
        }
    }
}