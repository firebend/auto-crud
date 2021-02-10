using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        /// <summary>
        /// Enables elastic pool connections to sql server
        /// </summary>
        /// <param name="configureShardMapManager">The type of the service to use</param>
        /// <param name="configure">The type of the service to use</param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithSearchFilter((f, s) => f.Summary.Contains(s))
        ///        .AddElasticPool(
        ///            manager => {
        ///                manager.ConnectionString = "connString";
        ///                manager.MapName = "your-map-name";
        ///                manager.Server = ".";
        ///                manager.ElasticPoolName = "pool-name";
        ///            },
        ///            pool => pool
        ///                .WithShardKeyProvider<KeyProvider>()
        ///                .WithShardDbNameProvider<DbNameProvider>()
        ///        )
        /// </code>
        /// </example>
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

            config.Builder.WithRegistration<
                IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>,
                AbstractConstraintUpdateExceptionHandler<TKey, TEntity>>();

            configure(config);

            return builder;
        }
    }
}
