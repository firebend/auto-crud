using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Implementations
{
    public class SearchExpressionProvider<TKey, TEntity, TSearch> : ISearchExpressionProvider<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        private readonly Func<TSearch, Expression<Func<TEntity, bool>>> _expression;

        public SearchExpressionProvider(Func<TSearch, Expression<Func<TEntity, bool>>> expression)
        {
            _expression = expression;
        }

        public Expression<Func<TEntity, bool>> GetSearchExpression(TSearch searchRequest) => _expression(searchRequest);
    }
}
