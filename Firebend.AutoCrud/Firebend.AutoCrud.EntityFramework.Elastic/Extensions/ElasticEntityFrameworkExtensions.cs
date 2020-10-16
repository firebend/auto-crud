using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            ShardMapMangerConfiguration shardConfiguration,
            bool isDev)
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
            
            if (isDev)
            {
                builder.WithRegistration<EntityFrameworkEntityBuilder, IShardManager, SqlServerShardManager>();
            }
            else
            {
                builder.WithRegistration<EntityFrameworkEntityBuilder, IShardManager, ShardManager>();
            }
            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                typeof(ShardDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.DbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));

            return builder;
        }
        
        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            bool isDev,
            Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(builder, shardConfig, isDev);
        }

        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            IHostEnvironment env,
            Action<ShardMapMangerConfiguration> configure)
        {
            return builder.WithElasticPool(env.IsDevelopment(), configure);
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
    }
}