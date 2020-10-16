using System;
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

        public IDbContext GetDbContext()
        {
            var key = _shardKeyProvider?.GetShardKey();
            
            var shard = _shardManager.RegisterShard(_shardMapMangerConfiguration,
                _shardNameProvider?.GetShardName(key),
                key);

            var connection = shard.OpenConnectionForKey(key, _shardMapMangerConfiguration.ConnectionString);
                
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
            
            context.Database.EnsureCreated();
            
            return context;
        }
    }
}