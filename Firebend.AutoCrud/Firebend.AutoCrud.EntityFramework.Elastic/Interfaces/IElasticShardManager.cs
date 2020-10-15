using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IElasticShardManager
    {
        ShardMap RegisterShard(ShardMapMangerConfiguration configuration, string shardDatabaseName, string key);
    }
}