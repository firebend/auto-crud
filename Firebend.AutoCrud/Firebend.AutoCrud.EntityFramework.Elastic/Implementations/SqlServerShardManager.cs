using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class SqlServerShardManager : IElasticShardManager
    {
        public ElasticDbShardModel RegisterShard(ShardMapMangerConfiguration configuration, string shardDatabaseName, string key)
            => new SqlServerElasticDbShardModel(shardDatabaseName);
    }
}