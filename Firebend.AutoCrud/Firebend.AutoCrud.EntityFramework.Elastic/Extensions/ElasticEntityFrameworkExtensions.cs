using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            ShardMapMangerConfiguration shardConfiguration)
        {
            if (shardConfiguration == null)
            {
                throw new ArgumentNullException(nameof(shardConfiguration));
            }

            if (builder.DbContextType == null)
            {
                throw new Exception("Please assign a db context type before using elastic pool");
            }
            
            builder.WithRegistrationInstance(shardConfiguration);
            builder.WithRegistration<EntityFrameworkEntityBuilder, IShardManager, ShardManager>();
            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                typeof(ShardDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.DbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));

            builder.WithDbCreator<DefaultDbCreator>();
            
            return builder;
        }
        
        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(builder, shardConfig);
        }

        public static EntityFrameworkEntityBuilder WithShardKeyProvider<TShardKeyProvider>(this EntityFrameworkEntityBuilder builder)
            where TShardKeyProvider : IShardKeyProvider
        {
            return builder.WithRegistration<EntityFrameworkEntityBuilder, IShardKeyProvider, TShardKeyProvider>();
        }
        
        public static EntityFrameworkEntityBuilder WithShardDbNameProvider<TShardDbNameProvider>(this EntityFrameworkEntityBuilder builder)
            where TShardDbNameProvider : IShardNameProvider
        {
            return builder.WithRegistration<EntityFrameworkEntityBuilder, IShardNameProvider, TShardDbNameProvider>();
        }

        public static EntityFrameworkEntityBuilder WithDbCreator<TDbCreator>(this EntityFrameworkEntityBuilder builder)
            where TDbCreator : IDbCreator
        {
            return builder.WithRegistration<EntityFrameworkEntityBuilder, IDbCreator, TDbCreator>();
        }
    }
}