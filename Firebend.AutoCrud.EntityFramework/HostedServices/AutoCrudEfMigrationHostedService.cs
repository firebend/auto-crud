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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoCrudEfMigrationHostedService<TContext>> _logger;

    public AutoCrudEfMigrationHostedService(IServiceProvider serviceProvider, ILogger<AutoCrudEfMigrationHostedService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await MigrateContext(_serviceProvider, _logger, stoppingToken);
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

    private static async Task MigrateContext(IServiceProvider provider, ILogger logger, CancellationToken stoppingToken)
    {
        using var scope = provider.CreateScope();

        var locker = scope.ServiceProvider.GetService<IDistributedLockService>();

        using var locked = await locker.LockAsync(nameof(AutoCrudEfMigrationHostedService<TContext>), stoppingToken);

        var connectionStringProvider = scope.ServiceProvider.GetService<IEntityFrameworkMigrationsConnectionStringProvider>();

        if (connectionStringProvider is null)
        {
            await MigrateUsingDefaultConnectionStringAsync(scope.ServiceProvider, logger, stoppingToken);
            return;
        }

        await MigrateUsingProvidedConnectionStringsAsync(connectionStringProvider, scope.ServiceProvider, logger, stoppingToken);
    }

    private static async Task MigrateUsingDefaultConnectionStringAsync(IServiceProvider provider,
        ILogger logger,
        CancellationToken stoppingToken)
    {
        var factory = provider.GetService<IDbContextFactory<TContext>>();

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
        IServiceProvider provider,
        ILogger logger,
        CancellationToken stoppingToken)
    {
        var connections = await connectionStringProvider.GetConnectionStringsAsync(stoppingToken);

        if (connections is null || connections.Length == 0)
        {
            logger.LogWarning("Connection string provider returned no strings.  Migrations will not be ran for any db contexts");
            return;
        }

        var dbContextProvider = provider.GetService<IDbContextFactory<TContext>>();

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
