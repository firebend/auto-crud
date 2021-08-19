using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Abstractions.Services
{
    public abstract class AbstractEntitySearchService<TEntity, TSearch> : BaseDisposable
        where TSearch : EntitySearchRequest
    {
        protected Expression<Func<TEntity, bool>> GetSearchExpression(TSearch search, Expression<Func<TEntity, bool>> customFilter = null)
        {
            var functions = new List<Expression<Func<TEntity, bool>>>();

            if (search is IActiveEntitySearchRequest activeEntitySearchRequest)
            {
                if (activeEntitySearchRequest.IsDeleted.HasValue)
                {
                    var expression = activeEntitySearchRequest.IsDeleted.Value
                        ? (Expression<Func<IActiveEntity, bool>>)(x => x.IsDeleted)
                        : (Expression<Func<IActiveEntity, bool>>)(x => !x.IsDeleted);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }
            }

            if (search is IModifiedEntitySearchRequest modifiedEntitySearchRequest)
            {
                if (modifiedEntitySearchRequest.CreatedStartDate.HasValue)
                {
                    var expression = (Expression<Func<IModifiedEntity, bool>>)(x => x.CreatedDate >= modifiedEntitySearchRequest.CreatedStartDate);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }

                if (modifiedEntitySearchRequest.CreatedEndDate.HasValue)
                {
                    var expression = (Expression<Func<IModifiedEntity, bool>>)(x => x.CreatedDate <= modifiedEntitySearchRequest.CreatedEndDate);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }

                if (modifiedEntitySearchRequest.ModifiedStartDate.HasValue)
                {
                    var expression = (Expression<Func<IModifiedEntity, bool>>)(x => x.ModifiedDate >= modifiedEntitySearchRequest.ModifiedStartDate);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }

                if (modifiedEntitySearchRequest.ModifiedEndDate.HasValue)
                {
                    var expression = (Expression<Func<IModifiedEntity, bool>>)(x => x.ModifiedDate <= modifiedEntitySearchRequest.ModifiedEndDate);
                    functions.Add(Expression.Lambda<Func<TEntity, bool>>(expression.Body, expression.Parameters));
                }
            }

            if (customFilter != null)
            {
                functions.Add(customFilter);
            }

            return functions.Aggregate(default(Expression<Func<TEntity, bool>>),
                (aggregate, filter) => aggregate.AndAlso(filter));
        }
    }
}
