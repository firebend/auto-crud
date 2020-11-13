using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface ISearchExpressionProvider<TKey, TEntity, TSearch>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TSearch : EntitySearchRequest
    {
        Expression<Func<TEntity, bool>> GetSearchExpression(TSearch searchRequest);
    }
}
