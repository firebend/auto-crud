using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class DbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    private readonly ILogger<DbContextProvider<TKey, TEntity, TContext>> _logger;
    private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IDbContextFactory<TContext> _contextFactory;

    private record InitContext(DbContext DbContext, ILogger Logger);

    public DbContextProvider(ILogger<DbContextProvider<TKey, TEntity, TContext>> logger,
        IDbContextFactory<TContext> contextFactory,
        IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider = null)
    {
        _connectionStringProvider = connectionStringProvider;
        _logger = logger;
        _contextFactory = contextFactory;
    }

    protected virtual void InitDb(DbContext dbContext)
        => AbstractDbContextProviderCache.InitCache.GetOrAdd(
            dbContext.Database.GetConnectionString(),
            InitDbCacheFactory,
            new InitContext(dbContext, _logger));

    private static bool InitDbCacheFactory(string connectionString, InitContext context)
    {
        try
        {
            context.DbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Fail to call migrations");
        }

        return true;
    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var providedConnectionString = await ProvideConnectionString(cancellationToken);

        if (!string.IsNullOrWhiteSpace(providedConnectionString))
        {
            var dbContextConnectionString = dbContext.Database.GetConnectionString();

            if (dbContextConnectionString != providedConnectionString)
            {
                await dbContext.Database.CloseConnectionAsync();
                dbContext.Database.SetConnectionString(providedConnectionString);
            }
        }

        InitDb(dbContext);

        return dbContext;
    }

    protected virtual async Task<string> ProvideConnectionString(CancellationToken cancellationToken)
    {
        if (_connectionStringProvider is null)
        {
            return null;
        }

        return await _connectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }

    public async Task<IDbContext> GetDbContextAsync(DbConnection connection,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await dbContext.Database.CloseConnectionAsync();
        dbContext.Database.SetDbConnection(connection);

        InitDb(dbContext);

        return dbContext;
    }
}
