namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces
{
    public interface IElasticShardManager
    {
        void RegisterShard(ShardMapMangerConfiguration configuration);
    }
}