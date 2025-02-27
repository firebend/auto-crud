using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IDefaultEntityOrderByProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public (Expression<Func<TEntity, object>> order, bool ascending) GetOrderBy();
}
