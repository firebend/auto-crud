using System;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Extensions
{
    public static class ElasticEntityFrameworkExtensions
    {
        public static EntityFrameworkEntityBuilder WithElasticPool(this EntityFrameworkEntityBuilder builder, ShardMapMangerConfiguration shardConfiguration)
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
            builder.WithRegistration<EntityFrameworkEntityBuilder, IElasticShardManager, ElasticShardManager>();

            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                typeof(ElasticDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.DbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));

            return builder;
        }
    }
}