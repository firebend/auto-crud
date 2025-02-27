using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoAllShardsProvider
{
    public Task<string[]> GetAllShardsAsync(CancellationToken cancellationToken);
}
