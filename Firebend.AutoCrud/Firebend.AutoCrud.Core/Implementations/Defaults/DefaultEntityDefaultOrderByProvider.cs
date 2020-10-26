using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultEntityDefaultOrderByProvider<TKey, TEntity> : IEntityDefaultOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public DefaultEntityDefaultOrderByProvider()
        {
            
        }

        public DefaultEntityDefaultOrderByProvider((Expression<Func<TEntity, object>>func, bool @ascending) orderBy)
        {
            OrderBy = orderBy;
        }
        
        public (Expression<Func<TEntity, object>>func, bool @ascending) OrderBy { get; set; }
    }
}