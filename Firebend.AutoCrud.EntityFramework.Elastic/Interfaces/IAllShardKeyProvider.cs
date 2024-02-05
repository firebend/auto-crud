using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IAllShardKeyProvider
{
    Task<string[]> GetAllShards(CancellationToken cancellationToken);
}
