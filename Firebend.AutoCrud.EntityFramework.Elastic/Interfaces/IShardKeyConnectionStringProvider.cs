using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardKeyConnectionStringProvider
{
    public Task<string> GetShardConnectionStringAsync(string shardKey, CancellationToken cancellationToken);
}
