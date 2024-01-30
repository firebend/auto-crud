using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client;

internal static class AbstractDbContextProviderCache
{
    public static readonly ConcurrentDictionary<string, Task<bool>> InitCache = new();
}

public abstract class AbstractDbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    private readonly ILogger _logger;
    private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IDbContextOptionsProvider<TKey, TEntity> _optionsProvider;

    private static readonly ConcurrentDictionary<string, PooledDbContextFactory<TContext>> PoolCache = new();

    private record InitContext(DbContext DbContext, ILogger Logger, CancellationToken CancellationToken);

    protected AbstractDbContextProvider(IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        IDbContextOptionsProvider<TKey, TEntity> optionsProvider,
        ILogger logger)
    {
        _connectionStringProvider = connectionStringProvider;
        _optionsProvider = optionsProvider;
        _logger = logger;
    }

    protected async Task<IDbContext> CreateContextAsync(IDbContextFactory<TContext> factory,
        CancellationToken cancellationToken)
    {
        var context = await factory.CreateDbContextAsync(cancellationToken);

        if (context is DbContext dbContext)
        {
            await AbstractDbContextProviderCache.InitCache.GetOrAdd(context.Database.GetConnectionString(),
                InitContextAsync,
                new InitContext(dbContext, _logger, cancellationToken));
        }

        return context;
    }

    private static async Task<bool> InitContextAsync(string connectionString, InitContext context)
    {
        try
        {
            await context.DbContext.Database.MigrateAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Fail to call migrations");
        }

        return true;
    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringProvider.GetConnectionStringAsync(cancellationToken);

        var options = _optionsProvider.GetDbContextOptions<TContext>(connectionString);

        var factory = PoolCache.GetOrAdd(connectionString,
            static (_, options) => new PooledDbContextFactory<TContext>(options),
            options);

        return await CreateContextAsync(factory, cancellationToken);
    }

    public async Task<IDbContext> GetDbContextAsync(DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        var options = _optionsProvider.GetDbContextOptions<TContext>(connection);

        var instance = Activator.CreateInstance(typeof(TContext), options) as TContext;

        await AbstractDbContextProviderCache.InitCache.GetOrAdd(connection.ConnectionString,
            InitContextAsync,
            new InitContext(instance, _logger, cancellationToken));

        return instance;
    }
}
