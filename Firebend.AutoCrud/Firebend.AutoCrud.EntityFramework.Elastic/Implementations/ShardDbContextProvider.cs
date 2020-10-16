using System;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class ShardDbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TContext : DbContext, IDbContext
    {
        private readonly IShardManager _shardManager;
        private readonly IShardKeyProvider _shardKeyProvider;
        private readonly IShardNameProvider _shardNameProvider;
        private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;

        public ShardDbContextProvider(
            IShardManager shardManager,
            IShardKeyProvider shardKeyProvider,
            IShardNameProvider shardNameProvider,
            ShardMapMangerConfiguration shardMapMangerConfiguration)
        {
            _shardManager = shardManager;
            _shardKeyProvider = shardKeyProvider;
            _shardNameProvider = shardNameProvider;
            _shardMapMangerConfiguration = shardMapMangerConfiguration;
        }

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var key = _shardKeyProvider?.GetShardKey();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("Shard key is null");
            }

            var shard = await _shardManager
                .RegisterShardAsync(_shardNameProvider?.GetShardName(key), key, cancellationToken)
                .ConfigureAwait(false);

            var keyBytes = Encoding.ASCII.GetBytes(key);
            
            var connStringBuilder = new SqlConnectionStringBuilder(_shardMapMangerConfiguration.ConnectionString);
            connStringBuilder.Remove("Data Source");
            connStringBuilder.Remove("Initial Catalog");
            
            var shardConnectionString = connStringBuilder.ConnectionString;
            
            var connection = await shard.OpenConnectionForKeyAsync(keyBytes, shardConnectionString)
                .ConfigureAwait(false);
                
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(connection)
                .Options;

            var instance = Activator.CreateInstance(typeof(TContext), options);
            
            if (instance == null)
            {
                throw new Exception("Could not create instance.");
            }

            if (!(instance is TContext context))
            {
                throw new Exception("Could not cast instance.");
            }
            
            await context.Database
                .EnsureCreatedAsync(cancellationToken)
                .ConfigureAwait(false);

            await context.Database
                .MigrateAsync(cancellationToken)
                .ConfigureAwait(false);
            
            return context;
        }
    }
}