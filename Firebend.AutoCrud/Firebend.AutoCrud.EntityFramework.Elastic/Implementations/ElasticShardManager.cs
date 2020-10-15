using System;
using System.Data.SqlClient;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class ElasticShardManager : IElasticShardManager
    {
        public ShardMap RegisterShard(ShardMapMangerConfiguration configuration, string shardDatabaseName, string key)
        {
            if (string.IsNullOrWhiteSpace(shardDatabaseName))
            {
                throw new Exception("Could not resolve a shard database name");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("Could not resolve key for shard.");
            }
            
            var manager = GetShardMapManger(configuration, shardDatabaseName);
            var shardMap = GetShardMap(manager, configuration);
            
            var shardLocation = new ShardLocation(configuration.Server, shardDatabaseName);

            if (!shardMap.TryGetShard(shardLocation, out var shard))
            {
                shard = shardMap.CreateShard(shardLocation);
            }

            if (!shardMap.TryGetMappingForKey(key, out _))
            {
                shardMap.CreatePointMapping(key, shard);
            }

            return shardMap;
        }

        private ShardMapManager GetShardMapManger(ShardMapMangerConfiguration configuration, string shardDatabaseName)
        {
            var connStringBuilder = new SqlConnectionStringBuilder(configuration.ConnectionString)
            {
                DataSource = configuration.Server,
                InitialCatalog = shardDatabaseName
            };

            if (ShardMapManagerFactory.TryGetSqlShardMapManager(
                connStringBuilder.ConnectionString,
                ShardMapManagerLoadPolicy.Lazy,
                out var manager))
            {
                return manager;
            }

            return ShardMapManagerFactory.CreateSqlShardMapManager(connStringBuilder.ConnectionString);
        }

        private ListShardMap<string> GetShardMap(ShardMapManager manager, ShardMapMangerConfiguration configuration)
        {
            if (manager.TryGetListShardMap<string>(configuration.MapName, out var listShardMap))
            {
                return listShardMap;
            }

            return manager.CreateListShardMap<string>(configuration.MapName);
        }
    }
}