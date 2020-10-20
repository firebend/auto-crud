using System;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

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

            if (Builder.DbContextType == null)
            {
                throw new Exception("Please assign a db context type before using elastic pool");
            }
            
            Builder.WithRegistrationInstance(shardConfiguration);
            Builder.WithRegistration<IShardManager, ShardManager>();
            Builder.WithRegistration<IDbContextProvider<TKey, TEntity>>(
                typeof(ShardDbContextProvider<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.DbContextType),
                false);

            WithDbCreator<DefaultDbCreator>();
            
            return this;
        }
        
        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithShardMapConfiguration(Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(shardConfig);
        }

        public ElasticPoolConfigurator<TBuilder, TKey, TEntity> WithShardKeyProvider<TShardKeyProvider>()
            where TShardKeyProvider : IShardKeyProvider
        {
             Builder.WithRegistration<IShardKeyProvider, TShardKeyProvider>();
             return this;
        }
        
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