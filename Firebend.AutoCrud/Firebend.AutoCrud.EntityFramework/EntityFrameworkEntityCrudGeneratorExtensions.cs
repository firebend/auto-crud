#region

using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Firebend.AutoCrud.EntityFramework
{
    public static class EntityFrameworkEntityCrudGeneratorExtensions
    {
        public static EntityFrameworkEntityCrudGenerator UsingEfCrud(this IServiceCollection serviceCollection)
        {
            return new EntityFrameworkEntityCrudGenerator(new DynamicClassGenerator(), serviceCollection);
        }
    }
}