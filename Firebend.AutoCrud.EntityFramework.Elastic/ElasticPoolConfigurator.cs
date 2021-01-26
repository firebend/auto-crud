using System;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;

namespace Firebend.AutoCrud.EntityFramework.Elastic
{
    public class ElasticPoolConfigurator<TBuilder, TKey, TEntity> : EntityBuilderConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityFrameworkEntityBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public ElasticPoolConfigurator(TBuilder builder) : base(builder)
        {
        }

        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithElasticPool(ShardMapMangerConfiguration shardConfiguration)
        {
            if (shardConfiguration == null)
            {
                throw new ArgumentNullException(nameof(shardConfiguration));
            }

            Builder.WithRegistrationInstance(shardConfiguration);
            Builder.WithRegistration<IShardManager, ShardManager>();
            Builder.WithConnectionStringProvider<ShardDbContextConnectionStringProvider<TKey, TEntity>>();

            WithDbCreator<DefaultDbCreator>();

            return this;
        }

        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithShardMapConfiguration(Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(shardConfig);
        }

        /// <summary>
        /// Specifies the ShardKeyProvider to use
        /// </summary>
        /// <typeparam name="TShardKeyProvider">The ShardKeyProvider to use</typeparam>
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
        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithShardKeyProvider<TShardKeyProvider>()
            where TShardKeyProvider : IShardKeyProvider
        {
            Builder.WithRegistration<IShardKeyProvider, TShardKeyProvider>();
            return this;
        }

        /// <summary>
        /// Specifies the ShardDbNameProvider to use
        /// </summary>
        /// <typeparam name="TShardDbNameProvider">The ShardDbNameProvider to use</typeparam>
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
        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithShardDbNameProvider<TShardDbNameProvider>()
            where TShardDbNameProvider : IShardNameProvider
        {
            Builder.WithRegistration<IShardNameProvider, TShardDbNameProvider>();
            return this;
        }

        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithDbCreator<TDbCreator>()
            where TDbCreator : IDbCreator
        {
            Builder.WithRegistration<IDbCreator, TDbCreator>();
            return this;
        }
    }
}
