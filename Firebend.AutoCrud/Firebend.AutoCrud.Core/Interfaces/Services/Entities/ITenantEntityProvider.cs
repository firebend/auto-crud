
using Firebend.AutoCrud.Core.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface ITenantEntityProvider<TKey> where TKey : struct
    {
        public Task<TenantEntityResult<TKey>> GetTenantAsync(CancellationToken cancellationToken = default);

    }
}
