using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

public class SampleAllShardsMongoProvider : IMongoAllShardsProvider
{
    public Task<string[]> GetAllShardsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(ShardKeyHelper.AllShards);
}
