using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Entities;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface ITenantEntityProvider<TKey>
    where TKey : struct
{
    public Task<TenantEntityResult<TKey>> GetTenantAsync(CancellationToken cancellationToken = default);
}
