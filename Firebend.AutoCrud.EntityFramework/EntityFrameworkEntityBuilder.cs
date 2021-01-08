using System;
using System.Linq;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Indexing;
using Firebend.AutoCrud.EntityFramework.Interfaces;

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

            WithRegistration<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>, DefaultEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkIncludesProvider<TKey, TEntity>, DefaultEntityFrameworkIncludesProvider<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>, DefaultEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>>(false);
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
        /// Adds a search filter for the entity
        /// </summary>
        /// <param name="type">The type of the search filter to use</param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast => 
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithSearchFilter(typeof(SearchFilter))
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter(Type type)
        {
            WithRegistration<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(type);
            return this;
        }

        /// <summary>
        /// Adds a search filter for the entity
        /// </summary>
        /// <typeparam name="T">The type of the search filter to use</typeparam>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast => 
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithSearchFilter<SearchFilter>()
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter<T>() => WithSearchFilter(typeof(T));

        /// <summary>
        /// Adds a search filter for the entity
        /// </summary>
        /// <param name="filter">A callback function returning whether to include the object in results for the search</param>
        /// <example>
        /// <code>
        /// ef.AddEntity<Guid, WeatherForecast>(forecast => 
        ///    forecast.WithDbContext<AppDbContext>()
        ///        .WithSearchFilter((e, s) => e.TemperatureC > s)
        ///        .AddCrud()
        ///        .AddControllers()
        /// </code>
        /// </example>
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter(Expression<Func<TEntity, string, bool>> filter)
        {
            WithRegistrationInstance<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(
                new DefaultEntityFrameworkFullTextExpressionProvider<TKey, TEntity>(filter));

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
