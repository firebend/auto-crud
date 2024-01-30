using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

public class SampleAllShardsMongoProvider : IMongoAllShardsProvider
{
    public Task<IEnumerable<string>> GetAllShardsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new[] { ShardKeyHelper.Firebend, ShardKeyHelper.FirebendBackwards }.AsEnumerable());
}
