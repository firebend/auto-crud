using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.HostedServices;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class DbContextProvider<TKey, TEntity, TContext> : IDbContextProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IDbContextFactory<TContext> _contextFactory;

    public DbContextProvider(IDbContextFactory<TContext> contextFactory,
        IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider = null)
    {
        _connectionStringProvider = connectionStringProvider;
        _contextFactory = contextFactory;
    }

    protected virtual void InitDb(DbContext dbContext)
    {

    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken)
    {
        await AutoCrudEfMigrationsMediator.HaveMigrationsRan.Task;

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

    public async Task<IDbContext> GetDbContextAsync(DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        await AutoCrudEfMigrationsMediator.HaveMigrationsRan.Task;

        var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.UseUserDefinedTransaction = true;
        dbContext.Database.SetDbConnection(transaction.Connection);
        await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

        InitDb(dbContext);

        return dbContext;
    }
}
