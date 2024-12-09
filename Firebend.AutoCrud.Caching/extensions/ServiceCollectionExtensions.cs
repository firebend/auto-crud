using Firebend.AutoCrud.Caching.implementations;
using Firebend.AutoCrud.Caching.interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Caching.extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection WithEntityCaching<TCacheOptions>(this IServiceCollection services)
        where TCacheOptions : class, IEntityCacheOptions
    {
        services.AddSingleton<IEntityCacheOptions, TCacheOptions>();

        if (services.All(x => x.ServiceType != typeof(IDistributedCache)))
        {
            throw new InvalidOperationException(
                "IDistributedCache is required for entity caching. Ensure it is registered before calling WithEntityCaching.");
        }

        return services;
    }

    public static IServiceCollection AddEntityCache<TKey, TEntity>(
        this IServiceCollection services)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        if (services.All(x => x.ServiceType != typeof(IEntityCacheOptions))) {
            throw new InvalidOperationException(
                "WithEntityCaching must be called before AddEntityCache.");
        }

        services.AddSingleton<IEntityCacheService<TKey, TEntity>, DefaultEntityCacheService<TKey, TEntity>>();

        return services;
    }
}
