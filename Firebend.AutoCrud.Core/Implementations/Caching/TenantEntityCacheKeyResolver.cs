#nullable enable
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

public class TenantEntityCacheKeyResolver(IServiceProvider serviceProvider) : ITenantEntityCacheKeyResolver
{
    private static readonly ConcurrentDictionary<Type, TenantEntityCacheKeyMetadata> TenantEntityCacheKeyMetadataCache = [];

    public async Task<string?> GetTenantIdSegmentAsync(Type entityType, CancellationToken cancellationToken)
    {
        var metadata = TenantEntityCacheKeyMetadataCache.GetOrAdd(entityType, CreateTenantEntityCacheKeyMetadata);

        if (metadata.GetTenantIdSegmentAsync is null)
        {
            return null;
        }

        return await metadata.GetTenantIdSegmentAsync(serviceProvider, cancellationToken);
    }

    private static TenantEntityCacheKeyMetadata CreateTenantEntityCacheKeyMetadata(Type entityType)
    {
        var tenantKeyType = entityType
            .GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITenantEntity<>))
            ?.GetGenericArguments()
            .FirstOrDefault();

        if (tenantKeyType is null)
        {
            return new TenantEntityCacheKeyMetadata(null);
        }

        var providerType = typeof(ITenantEntityProvider<>).MakeGenericType(tenantKeyType);
        var getTenantIdSegmentAsync = (Func<IServiceProvider, CancellationToken, Task<string>>)typeof(TenantEntityCacheKeyResolver)
            .GetMethod(nameof(CreateGetTenantIdSegmentAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(tenantKeyType)
            .Invoke(null, [providerType])!;
        return new TenantEntityCacheKeyMetadata(getTenantIdSegmentAsync);
    }

    private static Func<IServiceProvider, CancellationToken, Task<string>> CreateGetTenantIdSegmentAsync<TTenantKey>(
        Type providerType)
        where TTenantKey : struct
    {
        return async (sp, cancellationToken) =>
        {
            var tenantProvider = sp.GetService(providerType) ?? throw new InvalidOperationException(
                    $"Tenant entity cache keys require a registered {providerType.FullName}.");

            var tenant = await ((ITenantEntityProvider<TTenantKey>)tenantProvider).GetTenantAsync(cancellationToken) ?? throw new InvalidOperationException(
                    $"Unable to resolve tenant for entity cache key using {providerType.FullName}.");

            var tenantId = tenant.TenantId;

            if (tenantId.Equals(default(TTenantKey)))
            {
                throw new InvalidOperationException(
                    $"Unable to resolve non-default tenant id for entity cache key using {providerType.FullName}.");
            }

            return $"{tenantId}";
        };
    }

    private sealed record TenantEntityCacheKeyMetadata(Func<IServiceProvider, CancellationToken, Task<string>>? GetTenantIdSegmentAsync);
}
