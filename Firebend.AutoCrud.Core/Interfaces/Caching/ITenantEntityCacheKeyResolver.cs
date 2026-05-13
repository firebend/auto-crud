#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface ITenantEntityCacheKeyResolver
{
    public Task<string?> GetTenantIdSegmentAsync(Type entityType, CancellationToken cancellationToken);
}
