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
        Action<EntityFrameworkEntityCrudGenerator> configure)
        where TContext : DbContext
    {
        serviceCollection.AddDbContextFactory<TContext>(dbContextOptionsBuilder);
        //serviceCollection.AddPooledDbContextFactory<TContext>(dbContextOptionsBuilder);

        using var ef = new EntityFrameworkEntityCrudGenerator(
            new DynamicClassGenerator(),
            serviceCollection,
            dbContextOptionsBuilder,
            typeof(TContext));

        configure(ef);

        return ef.Generate();
    }
}
