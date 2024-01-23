using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Data.SqlClient;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions
{
    public abstract class AbstractShardManager : IShardManager
    {
        private readonly IDbCreator _dbCreator;
        private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;

        protected AbstractShardManager(IDbCreator dbCreator,
            ShardMapMangerConfiguration shardMapMangerConfiguration)
        {
            _dbCreator = dbCreator;
            _shardMapMangerConfiguration = shardMapMangerConfiguration;
        }

        public async Task<ShardMap> RegisterShardAsync(string shardDatabaseName, string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(shardDatabaseName))
            {
                throw new Exception("Could not resolve a shard database name");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("Could not resolve key for shard.");
            }

            var manager = await GetShardMapMangerAsync(cancellationToken).ConfigureAwait(false);
            var shardMap = GetShardMap(manager);

            await _dbCreator.EnsureCreatedAsync(_shardMapMangerConfiguration.ConnectionString,
                    shardDatabaseName,
                    cancellationToken)
                .ConfigureAwait(false);

            var shardLocation = new ShardLocation(_shardMapMangerConfiguration.Server, shardDatabaseName);

            if (!shardMap.TryGetShard(shardLocation, out var shard))
            {
                shard = shardMap.CreateShard(shardLocation);
            }

            var keyBytes = Encoding.ASCII.GetBytes(key);

            if (!shardMap.TryGetMappingForKey(keyBytes, out _))
            {
                shardMap.CreatePointMapping(keyBytes, shard);
            }

            return shardMap;
        }

        private async Task<ShardMapManager> GetShardMapMangerAsync(CancellationToken cancellationToken)
        {
            await _dbCreator.EnsureCreatedAsync(_shardMapMangerConfiguration.ConnectionString,
                    _shardMapMangerConfiguration.ShardMapManagerDbName,
                    cancellationToken)
                .ConfigureAwait(false);

            var connStringBuilder = new SqlConnectionStringBuilder(_shardMapMangerConfiguration.ConnectionString)
            {
                DataSource = _shardMapMangerConfiguration.Server,
                InitialCatalog = _shardMapMangerConfiguration.ShardMapManagerDbName
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

        private ListShardMap<byte[]> GetShardMap(ShardMapManager manager)
        {
            if (manager.TryGetListShardMap<byte[]>(_shardMapMangerConfiguration.MapName, out var listShardMap))
            {
                return listShardMap;
            }

            return manager.CreateListShardMap<byte[]>(_shardMapMangerConfiguration.MapName);
        }
    }
}
