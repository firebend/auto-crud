namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IElasticShardManager
    {
        ElasticDbShardModel RegisterShard(ShardMapMangerConfiguration configuration, string shardDatabaseName, string key);
    }
}