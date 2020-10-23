using System;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework
{
    public static class EntityFrameworkEntityCrudGeneratorExtensions
    {
        public static EntityFrameworkEntityCrudGenerator UsingEfCrud(this IServiceCollection serviceCollection)
        {
            return new EntityFrameworkEntityCrudGenerator(new DynamicClassGenerator(), serviceCollection);
        }

        public static IServiceCollection UsingEfCrud(this IServiceCollection serviceCollection,
            Action<EntityFrameworkEntityCrudGenerator> configure)
        {
            var ef = UsingEfCrud(serviceCollection);
            configure(ef);
            return ef.Generate();
        }
    }
}