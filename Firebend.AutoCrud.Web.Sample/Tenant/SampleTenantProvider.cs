using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Web.Sample.Tenant;

public class SampleTenantProvider : ITenantEntityProvider<int>
{
    public Task<TenantEntityResult<int>> GetTenantAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new TenantEntityResult<int> { TenantId = 100 });
}
