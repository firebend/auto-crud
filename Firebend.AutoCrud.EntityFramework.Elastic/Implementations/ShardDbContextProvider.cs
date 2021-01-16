using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    internal static class ShardDbContextProviderStatics
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
    public class ShardDbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TContext : DbContext, IDbContext
    {
        private readonly IShardKeyProvider _shardKeyProvider;
        private readonly IShardManager _shardManager;
        private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;
        private readonly IShardNameProvider _shardNameProvider;
        private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;

        public ShardDbContextProvider(
            IShardManager shardManager,
            IShardKeyProvider shardKeyProvider,
            IShardNameProvider shardNameProvider,
            ShardMapMangerConfiguration shardMapMangerConfiguration,
            IDbContextOptionsProvider<TKey, TEntity> optionsProvider)
        {
            _shardManager = shardManager;
            _shardKeyProvider = shardKeyProvider;
            _shardNameProvider = shardNameProvider;
            _shardMapMangerConfiguration = shardMapMangerConfiguration;
            _optionsProvider = optionsProvider;
        }

        public static string NormalizeToLegacyConnectionString(string connectionString)
            => string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : ShardDbContextProviderStatics
                .SqlPropertyRenames
                .Aggregate(connectionString,
                    (connString, replace) => connString.Replace(replace.newName, replace.oldName, StringComparison.OrdinalIgnoreCase));

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var key = _shardKeyProvider?.GetShardKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("Shard key is null");
            }

            var contextType = typeof(TContext);

            var shardRunKey = AutoCrudObjectPool.InterpolateString(GetType().FullName,
                ".",
                contextType.Name,
                ".ConnectionString.",
                key);

            var connectionString = await Run.OnceAsync(shardRunKey, async ct =>
                {
                    var shard = await _shardManager
                        .RegisterShardAsync(_shardNameProvider?.GetShardName(key), key, ct)
                        .ConfigureAwait(false);

                    var keyBytes = Encoding.ASCII.GetBytes(key);

                    var rootConnectionStringBuilder = new SqlConnectionStringBuilder(_shardMapMangerConfiguration.ConnectionString);
                    rootConnectionStringBuilder.Remove("Data Source");
                    rootConnectionStringBuilder.Remove("Initial Catalog");

                    var shardConnectionString = NormalizeToLegacyConnectionString(rootConnectionStringBuilder.ConnectionString);

                    await using var connection = await shard.OpenConnectionForKeyAsync(keyBytes, shardConnectionString)
                        .ConfigureAwait(false);

                    var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString) { Password = rootConnectionStringBuilder.Password };

                    return connectionStringBuilder.ConnectionString;
                }, cancellationToken)
                .ConfigureAwait(false);

            var options = _optionsProvider.GetDbContextOptions(connectionString);

            var instance = Activator.CreateInstance(contextType, options);

            if (instance == null)
            {
                throw new Exception("Could not create instance.");
            }

            if (!(instance is TContext context))
            {
                throw new Exception("Could not cast instance.");
            }

            var shardEnsureCreatedKey = AutoCrudObjectPool.InterpolateString(GetType().FullName,
                ".",
                contextType.Name,
                ".Init");

            await Run.OnceAsync(shardEnsureCreatedKey, async ct =>
                {
                    await context.Database
                        .EnsureCreatedAsync(ct)
                        .ConfigureAwait(false);

                    await context.Database
                        .MigrateAsync(ct)
                        .ConfigureAwait(false);
                }, cancellationToken)
                .ConfigureAwait(false);

            return context;
        }
    }
}
