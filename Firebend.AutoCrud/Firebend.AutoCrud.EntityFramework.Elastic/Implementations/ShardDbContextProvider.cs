using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
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

            var shard = await _shardManager
                .RegisterShardAsync(_shardNameProvider?.GetShardName(key), key, cancellationToken)
                .ConfigureAwait(false);
            
            var connection = await shard.OpenConnectionForKeyAsync(key, _shardMapMangerConfiguration.ConnectionString)
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
            
            return context;
        }
    }
}