using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Implementations.Caching;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterAllTypes<T>(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        var typesFromAssemblies =
            assemblies.SelectMany(a
                => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));

        foreach (var type in typesFromAssemblies)
        {
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
        }
    }

    public static IServiceCollection WithEntityCaching(this IServiceCollection services, Action<EntityCacheOptions> configure = null)
    {
        var cacheOptions = new EntityCacheOptions();
        configure?.Invoke(cacheOptions);
        services.AddScoped<IEntityCacheOptions>((_) => cacheOptions);
        CheckDistributedCache(services);
        return services;
    }

    public static IServiceCollection WithEntityCaching<TCacheOptions>(this IServiceCollection services)
        where TCacheOptions : class, IEntityCacheOptions
    {
        services.AddScoped<IEntityCacheOptions, TCacheOptions>();
        CheckDistributedCache(services);
        return services;
    }

    private static void CheckDistributedCache(IServiceCollection services)
    {
        if (services.All(x => x.ServiceType != typeof(IDistributedCache)))
        {
            throw new InvalidOperationException(
                "IDistributedCache is required for entity caching. Ensure it is registered before calling AddEntityCaching.");
        }
    }
}
