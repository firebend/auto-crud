using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Client;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

public class ChangeTrackingDbContextProvider<TEntityKey, TEntity>(
    IDbContextFactory<ChangeTrackingDbContext<TEntityKey, TEntity>> contextFactory,
    ILogger<ChangeTrackingDbContextProvider<TEntityKey, TEntity>> logger,
    IChangeTrackingTableNameProvider<TEntityKey, TEntity> tableNameProvider,
    IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider = null)
    :
        DbContextProvider<Guid, ChangeTrackingEntity<TEntityKey, TEntity>,
            ChangeTrackingDbContext<TEntityKey, TEntity>>(contextFactory),
        IChangeTrackingDbContextProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    private record ScaffoldCacheContext(DbContext DbContext, TableNameResult TableName, ILogger Logger);

    protected override bool WaitForMigrations => false;

    protected override void InitDb(DbContext dbContext)
    {
        base.InitDb(dbContext);
        var tableName = tableNameProvider.GetTableName();
        ScaffoldDbContext(dbContext, tableName, logger);
    }

    protected override async Task<string> ProvideConnectionString(CancellationToken cancellationToken)
    {
        if (connectionStringProvider is null)
        {
            return null;
        }

        return await connectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }

    private static void ScaffoldDbContext(DbContext context, TableNameResult tableName, ILogger logger)
    {
        var dbConn = context.Database.GetDbConnection();
        var cacheKey = $"{dbConn.DataSource}_{dbConn.Database}_{typeof(TEntity).FullName}";

        ChangeTrackingDbContextProviderCache.ScaffoldCache.GetOrAdd(cacheKey,
            ScaffoldCacheFactory,
            new ScaffoldCacheContext(context, tableName, logger));
    }

    private static bool ScaffoldCacheFactory(string typeName, ScaffoldCacheContext scaffoldCacheContext)
    {
        var changeTrackingType = typeof(ChangeTrackingEntity<TEntityKey, TEntity>);

        var schema = scaffoldCacheContext.TableName.Schema;
        var table = scaffoldCacheContext.TableName.Table;

        if (string.IsNullOrEmpty(table))
        {
            scaffoldCacheContext.Logger.LogWarning("Could not find table name for {TypeName}", changeTrackingType.FullName);
            return false;
        }

        try
        {
            if (scaffoldCacheContext.DbContext.Database.GetService<IDatabaseCreator>() is not RelationalDatabaseCreator dbCreator)
            {
                return true;
            }

            var exists = DoesTableExist(scaffoldCacheContext.DbContext, schema, table);

            if (!exists)
            {
                dbCreator.CreateTables();
            }

            return true;
        }
        catch (Exception ex)
        {
            scaffoldCacheContext.Logger.LogError(ex, "Error scaffolding change tracking table for {TypeName}", changeTrackingType.FullName);
            return true;
        }
    }

    private static bool DoesTableExist(DbContext context, string schemaName, string tableName)
    {
        var conn = context.Database.GetDbConnection();

        if (conn.State == ConnectionState.Closed)
        {
            conn.Open();
        }

        using var command = conn.CreateCommand();

        command.CommandText = $"""
                                   SELECT 1 FROM sys.tables AS T
                                       INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                                   WHERE S.Name = '{schemaName}' AND T.Name = '{tableName}'
                               """;
        var result = command.ExecuteScalar();
        return result != null;
    }
}
