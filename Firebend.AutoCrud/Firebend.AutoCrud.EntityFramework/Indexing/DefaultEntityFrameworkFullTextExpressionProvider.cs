#region

using System;
using Firebend.AutoCrud.EntityFramework.Interfaces;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Indexing
{
    public abstract class DefaultEntityFrameworkFullTextExpressionProvider : IEntityFrameworkFullTextExpressionProvider
    {
        public bool HasValue { get; } = false;

        public bool GetFullTextFilter<TEntity>(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}