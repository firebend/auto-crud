using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IAllShardKeyProvider
{
    public Task<string[]> GetAllShards(CancellationToken cancellationToken);
}
