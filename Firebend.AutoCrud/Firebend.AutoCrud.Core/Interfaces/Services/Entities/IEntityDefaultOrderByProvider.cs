using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityDefaultOrderByProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public (Expression<Func<TEntity, object>>func, bool ascending) OrderBy { get; }
    }
}