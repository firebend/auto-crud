using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Mongo.Sample.Tenant
{
    public class SampleTenantProvider : ITenantEntityProvider<int>
    {
        public Task<TenantEntityResult<int>> GetTenantAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new TenantEntityResult<int>() { TenantId = 100 });
        }
    }
}