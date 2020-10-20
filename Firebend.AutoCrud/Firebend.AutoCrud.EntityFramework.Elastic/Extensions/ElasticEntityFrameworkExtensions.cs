using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        
        public static TBuilder AddElasticPool<TBuilder>(this TBuilder builder,
            Action<ElasticPoolConfigurator<TBuilder>> configure)
            where TBuilder : EntityFrameworkEntityBuilder
        {
            return builder.AddElasticPool(null, configure);
        }
        
        public static TBuilder AddElasticPool<TBuilder>(this TBuilder builder,
            Action<ShardMapMangerConfiguration> configureShardMapManager,
            Action<ElasticPoolConfigurator<TBuilder>> configure)
            where TBuilder : EntityFrameworkEntityBuilder
        {
            var config = new ElasticPoolConfigurator<TBuilder>(builder);
            if (configureShardMapManager != null)
            {
                config.WithShardMapConfiguration(configureShardMapManager);
            }
            configure(config);
            return builder;
        }
        
    }
}