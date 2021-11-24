using System;
using System.Data.Common;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Connections;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Implementations;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityFrameworkEntityBuilder<TKey, TEntity> : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public EntityFrameworkEntityBuilder()
        {
            CreateType = typeof(EntityFrameworkEntityCreateService<TKey, TEntity>);
            ReadType = typeof(EntityFrameworkEntityReadService<TKey, TEntity>);
            UpdateType = typeof(EntityFrameworkEntityUpdateService<TKey, TEntity>);

            DeleteType = IsActiveEntity
                ? typeof(EntityFrameworkEntitySoftDeleteService<,>).MakeGenericType(EntityKeyType, EntityType)
                : typeof(EntityFrameworkEntityDeleteService<TKey, TEntity>);

            SearchType = typeof(EntityFrameworkEntitySearchService<,,>);
        }

        public override Type CreateType { get; }

        public override Type ReadType { get; }

        public override Type SearchType { get; }

        public override Type UpdateType { get; }

        public override Type DeleteType { get; }

        public Type DbContextType { get; set; }

        protected override void ApplyPlatformTypes()
        {
            if (IsTenantEntity)
            {
                WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantCreateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantQueryClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantUpdateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkDeleteClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantDeleteClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);
            }
            else
            {
                WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>, EntityFrameworkCreateClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>, EntityFrameworkQueryClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>, EntityFrameworkUpdateClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkDeleteClient<TKey, TEntity>, EntityFrameworkDeleteClient<TKey, TEntity>>(false);
            }

            WithRegistration<IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>,
                DefaultEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>>(false);

            WithRegistration<IEntityFrameworkIncludesProvider<TKey, TEntity>,
                DefaultEntityFrameworkIncludesProvider<TKey, TEntity>>(false);

            WithRegistration<IEntityTransactionFactory<TKey, TEntity>,
                EntityFrameworkEntityTransactionFactory<TKey, TEntity>>();
        }

        /// <summary>
        /// Specifies the DbContext to use for an entity
        /// </summary>
        /// <param name="dbContextType">The type of the DbContext to use</param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext(typeof(AppDbContext))
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbContext(Type dbContextType)
        {
            DbContextType = dbContextType;

            var t = typeof(DbContextProvider<,,>).MakeGenericType(EntityKeyType, EntityType, dbContextType);

            WithRegistration<IDbContextProvider<TKey, TEntity>>(t);

            return this;
        }

        /// <summary>
        /// Specifies the DbContext to use for an entity
        /// </summary>
        /// <typeparam name="TContext">The type of the DbContext to use</typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbContext<TContext>()
            where TContext : IDbContext => WithDbContext(typeof(TContext));

        private Type GetDbContextProviderType()
        {
            if (DbContextType is null)
            {
                throw new Exception("Please configure a context type first");
            }

            var providerType = typeof(IDbContextOptionsProvider<,,>).MakeGenericType(EntityKeyType, EntityType, DbContextType);
            return providerType;
        }

        /// <summary>
        /// Specifies EntityFramework Db Context options. Used when creating a change tracking context or a sharded context.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> that specifies a class that implements <see cref="IDbContextOptionsProvider{TKey,TEntity}"/>
        /// </param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithDbOptionsProvider(typeof(AppDbContextOptionsProvider))
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbOptionsProvider(Type type)
        {
            WithRegistration(GetDbContextProviderType(), type);
            return this;
        }

        /// <summary>
        /// Specifies EntityFramework Db Context options. Used when creating a change tracking context or a sharded context.
        /// </summary>
        /// <typeparam name="TProvider">The type that implements <see cref="IDbContextOptionsProvider{TKey,TEntity}"/></typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithDbOptionsProvider<AppDbContextOptionsProvider>()
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbOptionsProvider<TProvider>()
        {
            WithRegistration(GetDbContextProviderType(), typeof(TProvider));
            return this;
        }

        /// <summary>
        /// Specifies EntityFramework Db Context options.
        /// </summary>
        /// <param name="dbContextOptionsFunc">
        /// A <see cref="Func{TResult}"/> that accepts the connection string and returns a <see cref="DbContextOptions"/>
        /// </param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithDbOptionsProvider(new DbContextOptionsBuilder().AddSqlServer().Options)
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbOptionsProvider<TContext>(
            Func<string, DbContextOptions<TContext>> dbContextOptionsFunc,
            Func<DbConnection, DbContextOptions<TContext>> dbContextOptionsConnectionFunc)
            where TContext : DbContext, IDbContext
        {
            WithRegistrationInstance<IDbContextOptionsProvider<TKey, TEntity, TContext>>(
                new DbContextOptionsProvider<TKey, TEntity, TContext>(dbContextOptionsFunc, dbContextOptionsConnectionFunc));

            return this;
        }

        /// <summary>
        /// Specifies the connection string to use for the db context associated to this crud builder.
        /// </summary>
        /// <param name="connectionString">
        /// A <see cref="string"/> represending the connection string for the <see cref="DbContext"/>
        /// </param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithDbOptionsProvider(new DbContextOptionsBuilder().AddSqlServer().Options)
        ///        .WithConnectionString(Configuration.ConnectionStrings["MyDbConnection"])
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithConnectionString(string connectionString)
        {
            WithRegistrationInstance<IDbContextConnectionStringProvider<TKey, TEntity>>(new DefaultDbContextConnectionStringProvider<TKey, TEntity>(connectionString));
            return this;
        }

        /// <summary>
        /// Specifies the connection string to use for the db context associated to this crud builder.
        /// </summary>
        /// <typeparam name="TProvider">The type that implements <see cref="IDbContextConnectionStringProvider{TKey,TEntity}"/></typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithDbOptionsProvider<AppDbContextOptionsProvider>()
        ///        .WithConnectionStringProvider<AppDbContextConnectionStringProvider>()
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithConnectionStringProvider<TProvider>()
            where TProvider : IDbContextConnectionStringProvider<TKey, TEntity>
        {
            WithRegistration<IDbContextConnectionStringProvider<TKey, TEntity>, TProvider>();
            return this;
        }

        /// <summary>
        /// Specifies EntityFramework related model Includes to use for the model
        /// </summary>
        /// <param name="type">The function includes provider to use</param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithIncludes(typeof(EntityIncludes))
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithIncludes(Type type)
        {
            WithRegistration<IEntityFrameworkIncludesProvider<TKey, TEntity>>(type);

            return this;
        }

        /// <summary>
        /// Specifies EntityFramework related model Includes to use for the model
        /// </summary>
        /// <typeparam name="TProvider">The function includes provider to use</typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithIncludes<FunctionIncludes>()
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithIncludes<TProvider>()
            where TProvider : IEntityFrameworkIncludesProvider<TKey, TEntity>
        {
            WithRegistration<IEntityFrameworkIncludesProvider<TKey, TEntity>, TProvider>();

            return this;
        }

        /// <summary>
        /// Specifies EntityFramework related model Includes to use for the model
        /// </summary>
        /// <typeparam name="func">A callback function specifying the related model Includes to use for the model</typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast =>
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithIncludes(forecasts => forecasts.Include(f => f.LastUpdatedBy))
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithIncludes(Func<IQueryable<TEntity>, IQueryable<TEntity>> func)
        {
            WithRegistrationInstance<IEntityFrameworkIncludesProvider<TKey, TEntity>>(
                new FunctionIncludesProvider<TKey, TEntity>(func));

            return this;
        }
    }
}
