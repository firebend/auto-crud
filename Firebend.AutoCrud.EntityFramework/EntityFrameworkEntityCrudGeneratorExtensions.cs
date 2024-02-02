using System;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework;

public static class EntityFrameworkEntityCrudGeneratorExtensions
{
    public static IServiceCollection UsingEfCrud<TContext>(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionsBuilder,
        Action<EntityFrameworkEntityCrudGenerator> configure,
        bool usePooled = true)
        where TContext : DbContext
    {
        if (usePooled)
        {
            serviceCollection.AddPooledDbContextFactory<TContext>(dbContextOptionsBuilder);
        }
        else
        {
            serviceCollection.AddDbContextFactory<TContext>(dbContextOptionsBuilder);
        }

        using var ef = new EntityFrameworkEntityCrudGenerator(
            new DynamicClassGenerator(),
            serviceCollection,
            dbContextOptionsBuilder,
            typeof(TContext));

        configure(ef);

        return ef.Generate();
    }
}
