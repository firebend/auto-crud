using System;
using Firebend.AutoCrud.Core.Implementations.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.EntityFramework;

public static class EntityFrameworkEntityCrudGeneratorExtensions
{
    public static EntityFrameworkEntityCrudGenerator UsingEfCrud(this IServiceCollection serviceCollection) =>
        new(new DynamicClassGenerator(), serviceCollection);

    public static IServiceCollection UsingEfCrud(this IServiceCollection serviceCollection,
        Action<EntityFrameworkEntityCrudGenerator> configure)
    {

        serviceCollection.TryAddSingleton<IMemoizer>(new Memoizer());

        using var ef = UsingEfCrud(serviceCollection);
        configure(ef);
        return ef.Generate();
    }
}
