using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        
        public static TBuilder AddElasticPool<TBuilder, TKey, TEntity>(this TBuilder builder,
            Action<ElasticPoolConfigurator<TBuilder, TKey, TEntity>> configure)
            where TBuilder : EntityFrameworkEntityBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
        {
            return builder.AddElasticPool(null, configure);
        }
        
        public static TBuilder AddElasticPool<TBuilder, TKey, TEntity>(this TBuilder builder,
            Action<ShardMapMangerConfiguration> configureShardMapManager,
            Action<ElasticPoolConfigurator<TBuilder, TKey, TEntity>> configure)
            where TBuilder : EntityFrameworkEntityBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
        {
            var config = new ElasticPoolConfigurator<TBuilder, TKey, TEntity>(builder);
            
            if (configureShardMapManager != null)
            {
                config.WithShardMapConfiguration(configureShardMapManager);
            }
            
            configure(config);
            return builder;
        }
        
    }
}