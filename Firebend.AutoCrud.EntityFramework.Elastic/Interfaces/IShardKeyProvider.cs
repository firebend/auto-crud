using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardKeyProvider
{
    Task<string> GetShardKeyAsync(CancellationToken cancellationToken);
}
