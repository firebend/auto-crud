using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Core.Pooling;

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

        public Expression<Func<TEntity, bool>> GetSearchExpression(TSearch searchRequest)
        {
            using var _ = AutoCrudDelegatePool.GetPooledFunction(_expression, searchRequest, out var func);
            return func();
        }
    }
}
