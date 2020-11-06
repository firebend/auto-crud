
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface ITenantEntityProvider<TKey> where TKey : struct
    {
        public Task<TenantEntityResult<TKey>> GetTenantAsync(CancellationToken cancellationToken = default);

    }

    public class TenantEntityResult<TKey> where TKey: struct
    {
        public TKey TenantId { get; set; }
    }

    public class DefaultTenantEntityProvider<TKey> : ITenantEntityProvider<TKey> where TKey : struct
    {
        public Task<TenantEntityResult<TKey>> GetTenantAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((TenantEntityResult<TKey>)null);
        }
    }
}
