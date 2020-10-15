using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class SqlServerShardManager : IElasticShardManager
    {
        private readonly IElasticShardDatabaseNameProvider _databaseNameProvider;

        public SqlServerShardManager(IElasticShardDatabaseNameProvider databaseNameProvider)
        {
            _databaseNameProvider = databaseNameProvider;
        }

        public ElasticDbShardModel RegisterShard(ShardMapMangerConfiguration configuration, string shardDatabaseName, string key)
            => new SqlServerElasticDbShardModel(_databaseNameProvider);
    }
}