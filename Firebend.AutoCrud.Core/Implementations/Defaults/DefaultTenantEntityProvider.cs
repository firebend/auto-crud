using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Implementations.Defaults
{
    public class DefaultTenantEntityProvider<TKey> : ITenantEntityProvider<TKey> where TKey : struct
    {
        public Task<TenantEntityResult<TKey>> GetTenantAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((TenantEntityResult<TKey>)null);
        }
    }
}
