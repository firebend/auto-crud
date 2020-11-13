using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        public static EntityFrameworkEntityBuilder<TKey, TEntity> AddElasticPool<TKey, TEntity>(this EntityFrameworkEntityBuilder<TKey, TEntity> builder,
            Action<ShardMapMangerConfiguration> configureShardMapManager,
            Action<ElasticPoolConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>> configure)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, new()
        {
            var config = new ElasticPoolConfigurator<EntityFrameworkEntityBuilder<TKey, TEntity>, TKey, TEntity>(builder);

            if (configureShardMapManager != null)
            {
                config.WithShardMapConfiguration(configureShardMapManager);
            }

            configure(config);
            return builder;
        }
    }
}
