using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.HostedServices;

public static class AutoCrudEfMigrationsMediator
{
    public static readonly TaskCompletionSource HaveMigrationsRan = new();
}

public class AutoCrudEfMigrationHostedService<TContext> : BackgroundService
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AutoCrudEfMigrationHostedService<TContext>> _logger;

    public AutoCrudEfMigrationHostedService(IServiceScopeFactory serviceScopeFactory,
        ILogger<AutoCrudEfMigrationHostedService<TContext>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await MigrateContext(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running migrations");
        }
        finally
        {
            AutoCrudEfMigrationsMediator.HaveMigrationsRan.TrySetResult();
        }
    }

    private async Task MigrateContext(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var locker = scope.ServiceProvider.GetService<IDistributedLockService>();

        using var locked = await locker.LockAsync(nameof(AutoCrudEfMigrationHostedService<TContext>), stoppingToken);

        var connectionStringProvider = scope.ServiceProvider.GetService<IEntityFrameworkMigrationsConnectionStringProvider>();

        if (connectionStringProvider is null)
        {
            await MigrateUsingDefaultConnectionStringAsync(scope, _logger, stoppingToken);
            return;
        }

        await MigrateUsingProvidedConnectionStringsAsync(connectionStringProvider, scope, _logger, stoppingToken);
    }

    private static async Task MigrateUsingDefaultConnectionStringAsync(IServiceScope scope,
        ILogger logger,
        CancellationToken stoppingToken)
    {
        var factory = scope.ServiceProvider.GetService<IDbContextFactory<TContext>>();

        await using var context = await factory.CreateDbContextAsync(stoppingToken);

        var connectionString = context.Database.GetConnectionString();

        try
        {
            await context.Database.MigrateAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            LogMigrationError(logger, connectionString, ex);
        }
    }

    private static async Task MigrateUsingProvidedConnectionStringsAsync(
        IEntityFrameworkMigrationsConnectionStringProvider connectionStringProvider,
        IServiceScope scope,
        ILogger logger,
        CancellationToken stoppingToken)
    {
        var connections = await connectionStringProvider.GetConnectionStringsAsync(stoppingToken);

        if (connections is null || connections.Length == 0)
        {
            logger.LogWarning("Connection string provider returned no strings.  Migrations will not be ran for any db contexts");
            return;
        }

        var dbContextProvider = scope.ServiceProvider.GetService<IDbContextFactory<TContext>>();

        foreach (var connectionString in connections)
        {
            try
            {
                await using var context = await dbContextProvider.CreateDbContextAsync(stoppingToken);
                await context.Database.CloseConnectionAsync();
                context.Database.SetConnectionString(connectionString);
                await context.Database.MigrateAsync(cancellationToken: stoppingToken);
                LogMigrationComplete(logger, connectionString);
            }
            catch (Exception ex)
            {
                LogMigrationError(logger, connectionString, ex);
            }
        }
    }

    private static void LogMigrationError(ILogger logger, string connectionString, Exception ex)
    {
        var connectionStringBuilder = new DbConnectionStringBuilder(false)
        {
            ConnectionString = connectionString
        };

        connectionStringBuilder.TryGetValue("Data Source", out var dataSource);
        connectionStringBuilder.TryGetValue("Initial Catalog", out var database);

        logger.LogError(ex, "Error running migrations. Context: {Context} Server: {Server} Db: {Db}",
            typeof(TContext).Name,
            dataSource,
            database);
    }

    private static void LogMigrationComplete(ILogger logger, string connectionString)
    {
        if (!logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        var connectionStringBuilder = new DbConnectionStringBuilder(false)
        {
            ConnectionString = connectionString
        };

        connectionStringBuilder.TryGetValue("Data Source", out var dataSource);
        connectionStringBuilder.TryGetValue("Initial Catalog", out var database);

        logger.LogDebug("Finished Migrating. Context: {Context} Server: {Server} Db: {Db}",
            typeof(TContext).Name,
            dataSource,
            database);
    }
}
