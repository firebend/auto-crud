using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardManager
{
    public Task<ShardMap> RegisterShardAsync(string shardDatabaseName,
        string key,
        CancellationToken cancellationToken);
}
