using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public class SampleKeyProvider : IShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }

    public class SampleKeyProviderMongo : IMongoShardKeyProvider
    {
        public string GetShardKey() => "Firebend";
    }

    public class SampleAllShardsMongoProvider : IMongoAllShardsProvider
    {
        public Task<IEnumerable<string>> GetAllShardsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new[] { "Firebend", "Dneberif" }.AsEnumerable());
    }
}
