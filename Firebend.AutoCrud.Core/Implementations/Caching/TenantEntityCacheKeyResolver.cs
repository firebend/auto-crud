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
    private readonly ConcurrentDictionary<Type, TenantEntityCacheKeyMetadata> _tenantEntityCacheKeyMetadata = [];

    public async Task<string?> GetTenantIdSegmentAsync(Type entityType, CancellationToken cancellationToken)
    {
        var metadata = _tenantEntityCacheKeyMetadata.GetOrAdd(entityType, CreateTenantEntityCacheKeyMetadata);

        if (metadata.GetTenantIdSegmentAsync is null)
        {
            return null;
        }

        return await metadata.GetTenantIdSegmentAsync(cancellationToken);
    }

    private TenantEntityCacheKeyMetadata CreateTenantEntityCacheKeyMetadata(Type entityType)
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
        var getTenantIdSegmentAsync = (Func<CancellationToken, Task<string>>)GetType()
            .GetMethod(nameof(CreateGetTenantIdSegmentAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(tenantKeyType)
            .Invoke(null, [serviceProvider, providerType])!;

        return new TenantEntityCacheKeyMetadata(getTenantIdSegmentAsync);
    }

    private static Func<CancellationToken, Task<string>> CreateGetTenantIdSegmentAsync<TTenantKey>(
        IServiceProvider serviceProvider,
        Type providerType)
        where TTenantKey : struct
    {
        return async cancellationToken =>
        {
            var tenantProvider = serviceProvider.GetService(providerType) ?? throw new InvalidOperationException(
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

    private sealed record TenantEntityCacheKeyMetadata(Func<CancellationToken, Task<string>>? GetTenantIdSegmentAsync);
}
