using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions
{
    internal static class ShardDbContextProviderCaches
    {
        public static readonly (string newName, string oldName)[] SqlPropertyRenames =
        {
            ("Application Intent", "ApplicationIntent"),
            ("Connect Retry Count", "ConnectRetryCount"),
            ("Connect Retry Interval", "ConnectRetryInterval"),
            ("Pool Blocking Period", "PoolBlockingPeriod"),
            ("Multiple Active Result Sets", "MultipleActiveResultSets"),
            ("Multi Subnet Failover", "MultiSubnetFailover"),
            ("Transparent Network IP Resolution", "TransparentNetworkIPResolution"),
            ("Trust Server Certificate", "TrustServerCertificate")
        };
    }

    public abstract class AbstractShardDbContextConnectionStringProvider<TKey, TEntity> : IDbContextConnectionStringProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IShardKeyProvider _shardKeyProvider;
        private readonly IShardManager _shardManager;
        private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;
        private readonly IShardNameProvider _shardNameProvider;
        private readonly IMemoizer<string> _memoizer;

        protected AbstractShardDbContextConnectionStringProvider(
            IShardManager shardManager,
            IShardKeyProvider shardKeyProvider,
            IShardNameProvider shardNameProvider,
            ShardMapMangerConfiguration shardMapMangerConfiguration,
            IMemoizer<string> memoizer)
        {
            _shardManager = shardManager;
            _shardKeyProvider = shardKeyProvider;
            _shardNameProvider = shardNameProvider;
            _shardMapMangerConfiguration = shardMapMangerConfiguration;
            _memoizer = memoizer;
        }

        private static string NormalizeToLegacyConnectionString(string connectionString)
            => string.IsNullOrWhiteSpace(connectionString)
                ? connectionString
                : ShardDbContextProviderCaches
                    .SqlPropertyRenames
                    .Aggregate(connectionString,
                        (connString, replace) => connString.Replace(replace.newName, replace.oldName, StringComparison.OrdinalIgnoreCase));

        public async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default)
        {
            var shardKey = _shardKeyProvider?.GetShardKey();

            if (string.IsNullOrWhiteSpace(shardKey))
            {
                throw new Exception("Shard key is null");
            }

            var memoizeKey = $"{shardKey}.Sharding.Enrollment";

            var connectionString = await _memoizer.MemoizeAsync<
                (AbstractShardDbContextConnectionStringProvider<TKey, TEntity> self, string shardKey, CancellationToken cancellationToken)>(
                memoizeKey,
                static arg => arg.self.GetShardConnectionStringAsync(arg.shardKey, arg.cancellationToken),
                (this, shardKey, cancellationToken),
                cancellationToken);

            return connectionString;
        }

        private async Task<string> GetShardConnectionStringAsync(string key, CancellationToken cancellationToken)
        {
            var shard = await _shardManager
                .RegisterShardAsync(_shardNameProvider?.GetShardName(key), key, cancellationToken)
                .ConfigureAwait(false);

            var keyBytes = Encoding.ASCII.GetBytes(key);

            var rootConnectionStringBuilder = new SqlConnectionStringBuilder(_shardMapMangerConfiguration.ConnectionString);
            rootConnectionStringBuilder.Remove("Data Source");
            rootConnectionStringBuilder.Remove("Initial Catalog");

            var shardConnectionString = NormalizeToLegacyConnectionString(rootConnectionStringBuilder.ConnectionString);

            await using var connection = await shard.OpenConnectionForKeyAsync(keyBytes, shardConnectionString)
                .ConfigureAwait(false);

            var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString) { Password = rootConnectionStringBuilder.Password };

            var connectionString = connectionStringBuilder.ConnectionString;
            connectionStringBuilder = null;
            rootConnectionStringBuilder = null;

            return connectionString;
        }
    }
}
