using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.HostedServices;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class DbContextProvider<TKey, TEntity, TContext>(
    IDbContextFactory<TContext> contextFactory,
    IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider = null)
    : IDbContextProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    protected virtual bool WaitForMigrations => true;

    protected virtual void InitDb(DbContext dbContext)
    {

    }

    protected virtual async Task<string> ProvideConnectionString(CancellationToken cancellationToken)
    {
        if (connectionStringProvider is null)
        {
            return null;
        }

        return await connectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }

    protected virtual async Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        var dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

        var providedConnectionString = await ProvideConnectionString(cancellationToken);

        if (string.IsNullOrWhiteSpace(providedConnectionString))
        {
            return dbContext;
        }

        var dbContextConnectionString = dbContext.Database.GetConnectionString();

        if (dbContextConnectionString != providedConnectionString)
        {
            await dbContext.Database.CloseConnectionAsync();
            dbContext.Database.SetConnectionString(providedConnectionString);
        }

        return dbContext;
    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken)
    {
        if (WaitForMigrations)
        {
            await AutoCrudEfMigrationsMediator.HaveMigrationsRan<TContext>().Task;
        }

        var dbContext = await CreateDbContextAsync(cancellationToken);

        InitDb(dbContext);

        return dbContext;
    }

    public async Task<IDbContext> GetDbContextAsync(DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        await AutoCrudEfMigrationsMediator.HaveMigrationsRan<TContext>().Task;

        var dbContext = await CreateDbContextAsync(cancellationToken);
        dbContext.UseUserDefinedTransaction = true;
        dbContext.Database.SetDbConnection(transaction.Connection);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        InitDb(dbContext);

        return dbContext;
    }
}
