using System.Data.Common;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Firebend.AutoCrud.EntityFramework.Elastic
{
    public class ElasticPoolShardModel : ElasticDbShardModel
    {
        private readonly ShardMap _shardMap;

        public ElasticPoolShardModel(ShardMap shardMap)
        {
            _shardMap = shardMap;
        }

        public override DbConnection OpenConnectionForKey(string key, string connectionString)
            => _shardMap.OpenConnectionForKey(key, connectionString);
    }
}