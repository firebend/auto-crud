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
                builder.WithRegistration<EntityFrameworkEntityBuilder, IElasticShardManager, SqlServerShardManager>();
            }
            else
            {
                builder.WithRegistration<EntityFrameworkEntityBuilder, IElasticShardManager, ElasticShardManager>();
            }
            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                typeof(ElasticDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.DbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));

            return builder;
        }

        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder,
            IHostEnvironment env,
            Action<ShardMapMangerConfiguration> configure)
        {
            var shardConfig = new ShardMapMangerConfiguration();
            configure(shardConfig);
            return WithElasticPool(builder, shardConfig, env.IsDevelopment());
        }

        public static EntityFrameworkEntityBuilder WithShardKeyProvider<TShardKeyProvider>(this EntityFrameworkEntityBuilder builder)
            where TShardKeyProvider : IElasticShardKeyProvider
        {
            return builder.WithRegistration<EntityFrameworkEntityBuilder, IElasticShardKeyProvider, TShardKeyProvider>();
        }
        
        public static EntityFrameworkEntityBuilder WithShardDbNameProvider<TShardDbNameProvider>(this EntityFrameworkEntityBuilder builder)
            where TShardDbNameProvider : IElasticShardDatabaseNameProvider
        {
            return builder.WithRegistration<EntityFrameworkEntityBuilder, IElasticShardDatabaseNameProvider, TShardDbNameProvider>();
        }
    }
}