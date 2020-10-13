namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkFullTextExpressionProvider
    {
        bool HasValue { get; }

        bool GetFullTextFilter<TEntity>(TEntity entity);
    }
}