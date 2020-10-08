using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityDefaultOrderByProvider<TEntity, TKey> : IEntityDefaultOrderByProvider<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public (Expression<Func<TEntity, object>>, bool @ascending) GetOrderBy() => default;
    }
}