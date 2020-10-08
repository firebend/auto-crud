using Firebend.AutoCrud.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudGenerator
    {
        void Generate<TEntity, TKey>(IServiceCollection collection, EntityCrudBuilder builder)
            where TEntity : IEntity<TKey>
            where TKey : struct;
    }
}