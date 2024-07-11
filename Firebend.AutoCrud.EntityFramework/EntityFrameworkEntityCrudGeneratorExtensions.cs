using System;
using Firebend.AutoCrud.EntityFramework.HostedServices;
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
        bool usePooled = false)
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

        serviceCollection.AddHostedService<AutoCrudEfMigrationHostedService<TContext>>();

        using var ef = new EntityFrameworkEntityCrudGenerator(
            new DynamicClassGenerator(),
            serviceCollection,
            dbContextOptionsBuilder,
            typeof(TContext),
            usePooled);

        configure(ef);

        return ef.Generate();
    }
}
