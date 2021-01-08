using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoAllShardsProvider
    {
        Task<IEnumerable<string>> GetAllShardsAsync(CancellationToken cancellationToken = default);
    }
}
