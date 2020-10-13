using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Indexing
{
    public class DefaultEntityFrameworkFullTextExpressionProvider: IEntityFrameworkFullTextExpressionProvider
    {
        public bool HasValue { get; } = false;
        
        public bool GetFullTextFilter<TEntity>(TEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}