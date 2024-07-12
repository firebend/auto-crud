using System;
using System.Linq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Client;
using Firebend.AutoCrud.EntityFramework.Connections;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Implementations;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework;

public class EntityFrameworkEntityBuilder<TKey, TEntity> : EntityCrudBuilder<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    public Type DbContextType { get; }
    public Action<IServiceProvider, DbContextOptionsBuilder> DbContextOptionsBuilder { get; }
    public bool UsePooled { get; }

    public EntityFrameworkEntityBuilder(IServiceCollection services,
        Type dbContextType,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionsBuilder,
        bool usePooled) : base(services)
    {
        DbContextType = dbContextType;
        DbContextOptionsBuilder = dbContextOptionsBuilder;
        UsePooled = usePooled;

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

        var dbContextProvider = typeof(DbContextProvider<,,>).MakeGenericType(EntityKeyType, EntityType, DbContextType);

        WithRegistration<IDbContextProvider<TKey, TEntity>>(dbContextProvider);
    }

    /// <summary>
    /// Specifies the connection string to use for the db context associated to this crud builder.
    /// </summary>
    /// <param name="connectionString">
    /// A <see cref="string"/> representing the connection string for the <see cref="DbContext"/>
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
    /// <param name="func">A callback function specifying the related model Includes to use for the model</typeparam>
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
