using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class ElasticDbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TContext : DbContext, IDbContext
    {
        private readonly IElasticShardManager _shardManager;
        private readonly IElasticShardKeyProvider _shardKeyProvider;
        private readonly IElasticShardDatabaseNameProvider _shardDatabaseNameProvider;
        private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;

        public ElasticDbContextProvider(
            IElasticShardManager shardManager,
            IElasticShardKeyProvider shardKeyProvider,
            IElasticShardDatabaseNameProvider shardDatabaseNameProvider,
            ShardMapMangerConfiguration shardMapMangerConfiguration)
        {
            _shardManager = shardManager;
            _shardKeyProvider = shardKeyProvider;
            _shardDatabaseNameProvider = shardDatabaseNameProvider;
            _shardMapMangerConfiguration = shardMapMangerConfiguration;
        }

        public IDbContext GetDbContext()
        {
            var key = _shardKeyProvider?.GetShardKey();
            
            var shard = _shardManager.RegisterShard(_shardMapMangerConfiguration,
                _shardDatabaseNameProvider?.GetShardDatabaseName(),
                key);

            var connection = shard.OpenConnectionForKey(key, _shardMapMangerConfiguration.ConnectionString);
                
            var options = new DbContextOptionsBuilder()
                .UseSqlServer(connection)
                .Options;

            var context = Activator.CreateInstance(typeof(TContext), options);
            
            return (TContext)context;
        }
    }
}