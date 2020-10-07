using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudGenerator
    {
        void Generate<TEntity, TKey>(IServiceCollection collection, IEntityCrudBuilder builder)
            where TEntity : IEntity<TKey>
            where TKey : struct;
    }
}