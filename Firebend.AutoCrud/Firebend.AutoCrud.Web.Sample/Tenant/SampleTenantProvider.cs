using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Web.Sample.Tenant
{
    public class SampleTenantProvider : ITenantEntityProvider<int>
    {
        public Task<TenantEntityResult<int>> GetTenantAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TenantEntityResult<int>() { TenantId = 100 });
        }
    }
}
