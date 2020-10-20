using System;
using Firebend.AutoCrud.Core.Abstractions.Configurators;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic
{
    public class ElasticPoolConfigurator<TBuilder> : BuilderConfigurator<TBuilder>
        where TBuilder : EntityFrameworkEntityBuilder
    {
        public ElasticPoolConfigurator(TBuilder builder) : base(builder)
        {
        }
        
        public ElasticPoolConfigurator<TBuilder> WithElasticPool(ShardMapMangerConfiguration shardConfiguration)
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
            Builder.WithRegistration<EntityFrameworkEntityBuilder, IShardManager, ShardManager>();
            Builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType),
                typeof(ShardDbContextProvider<,,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType, Builder.DbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(Builder.EntityKeyType, Builder.EntityType));

            WithDbCreator<DefaultDbCreator>();
            
            return this;
        }
        
        public ElasticPoolConfigurator<TBuilder> WithShardMapConfiguration(Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(shardConfig);
        }

        public ElasticPoolConfigurator<TBuilder> WithShardKeyProvider<TShardKeyProvider>()
            where TShardKeyProvider : IShardKeyProvider
        {
             Builder.WithRegistration<EntityFrameworkEntityBuilder, IShardKeyProvider, TShardKeyProvider>();
             return this;
        }
        
        public ElasticPoolConfigurator<TBuilder> WithShardDbNameProvider<TShardDbNameProvider>()
            where TShardDbNameProvider : IShardNameProvider
        {
            Builder.WithRegistration<EntityFrameworkEntityBuilder, IShardNameProvider, TShardDbNameProvider>();
            return this;
        }

        public ElasticPoolConfigurator<TBuilder> WithDbCreator<TDbCreator>()
        {
            Builder.WithRegistration<EntityFrameworkEntityBuilder, IDbCreator, TDbCreator>();
            return this;
        }
    }
}