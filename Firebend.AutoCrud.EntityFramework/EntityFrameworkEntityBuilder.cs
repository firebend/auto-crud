using System;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Connections;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Querying;
using Microsoft.EntityFrameworkCore;

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

        private Type _searchType;

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

            if (!HasRegistration(typeof(IEntityFrameworkQueryableCustomizer<,,>).MakeGenericType(EntityKeyType, EntityType, SearchRequestType)))
            {
                WithQueryableCustomizer(typeof(DefaultEntityFrameworkQueryCustomizer<,,>).MakeGenericType(
                    EntityKeyType,
                    EntityType,
                    SearchRequestType));
            }

            EnsureRegistered<IDbContextConnectionStringProvider<TKey, TEntity>>();
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
            WithRegistration<IDbContextOptionsProvider<TKey, TEntity>>(type);
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
        where TProvider : IDbContextOptionsProvider<TKey, TEntity>
        {
            WithRegistration<IDbContextOptionsProvider<TKey, TEntity>, TProvider>();
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
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbOptionsProvider(Func<string, DbContextOptions> dbContextOptionsFunc)
        {
            WithRegistrationInstance<IDbContextOptionsProvider<TKey, TEntity>>(new DbContextOptionsProvider<TKey, TEntity>(dbContextOptionsFunc));
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

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithQueryableCustomizer(Type type)
        {
            var service = typeof(IEntityFrameworkQueryableCustomizer<,,>).MakeGenericType(EntityKeyType,
                EntityType,
                SearchRequestType);

            WithRegistration(service, type);

            return this;
        }

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithQueryableCustomizer<TCustomizer>()
            => WithQueryableCustomizer(typeof(TCustomizer));

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithQueryableCustomizer<TSearch>(Func<IQueryable<TEntity>, TSearch, IQueryable<TEntity>> func)
            where TSearch : EntitySearchRequest
        {
            SearchRequestType = typeof(TSearch);
            var instance = new DefaultEntityFrameworkQueryCustomizer<TKey, TEntity, TSearch>(func);
            WithRegistrationInstance<IEntityFrameworkQueryableCustomizer<TKey, TEntity, TSearch>>(instance);

            return this;
        }
    }
}
