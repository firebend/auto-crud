using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;

internal static class AbstractChangeTrackingDbContextProviderCache
{
    public static readonly ConcurrentDictionary<string, Task<bool>> ScaffoldCache = new();
}

public abstract class AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext> :
    IChangeTrackingDbContextProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
    where TContext : DbContext, IDbContext
{
    private readonly IChangeTrackingOptionsProvider<TEntityKey, TEntity> _changeTrackingOptionsProvider;
    private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _connectionStringProvider;
    private readonly IDbContextOptionsProvider<TEntityKey, TEntity> _optionsProvider;
    private record ScaffoldContext(DbContext DbContext, bool PersistCustomContext, CancellationToken CancellationToken);

    protected AbstractChangeTrackingDbContextProvider(IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
        IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider,
        IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider)
    {
        _optionsProvider = optionsProvider;
        _connectionStringProvider = connectionStringProvider;
        _changeTrackingOptionsProvider = changeTrackingOptionsProvider;
    }

    public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringProvider
            .GetConnectionStringAsync(cancellationToken)
            .ConfigureAwait(false);

        var options = _optionsProvider.GetDbContextOptions<ChangeTrackingDbContext<TEntityKey, TEntity>>(connectionString);

        var context = await GetDbContextAsync(options, cancellationToken);

        return context;
    }

    public async Task<IDbContext> GetDbContextAsync(DbConnection connection, CancellationToken cancellationToken = default)
    {
        var options = _optionsProvider.GetDbContextOptions<ChangeTrackingDbContext<TEntityKey, TEntity>>(connection);

        var context = await GetDbContextAsync(options, cancellationToken);

        return context;
    }

    private async Task<IDbContext> GetDbContextAsync(DbContextOptions options, CancellationToken cancellationToken)
    {
        var context = new ChangeTrackingDbContext<TEntityKey, TEntity>(options, _changeTrackingOptionsProvider);

        await AbstractChangeTrackingDbContextProviderCache.ScaffoldCache.GetOrAdd(typeof(TEntity).FullName,
            ScaffoldAsync,
            new ScaffoldContext(context, _changeTrackingOptionsProvider?.Options?.PersistCustomContext ?? false, cancellationToken)
        );

        return context;
    }

    private static async Task<bool> ScaffoldAsync(string typeName, ScaffoldContext context)
    {
        var type = context.DbContext.Model.FindEntityType(typeof(ChangeTrackingEntity<TEntityKey, TEntity>))
                   ?? throw new Exception("Could not find entity type.");

        var schema = type.GetSchema().Coalesce("dbo");
        var table = type.GetTableName();

        var fullTableName = $"[{schema}].[{table}]";

        if (context.DbContext.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
        {
            var exists = await DoesTableExist(context.DbContext, schema, table, context.CancellationToken);

            if (!exists)
            {
                await dbCreator
                    .CreateTablesAsync(context.CancellationToken)
                    .ConfigureAwait(false);
            }
        }

        if (context.PersistCustomContext)
        {
            await AddMigrationFieldsAsync(context.DbContext, fullTableName, context.CancellationToken);
        }

        return true;
    }

    private static async Task AddMigrationFieldsAsync(DbContext context,
        string fullTableName,
        CancellationToken cancellationToken)
    {
        //todo: refactor this later to be more robust and not depend on sql server syntax
        const string columnName = nameof(ChangeTrackingEntity<TEntityKey, TEntity>.DomainEventCustomContext);

#pragma warning disable EF1002
        // ReSharper disable once UseRawString
        await context.Database.ExecuteSqlRawAsync($@"
IF NOT EXISTS (
  SELECT *
  FROM   sys.columns
  WHERE  object_id = OBJECT_ID(N'{fullTableName}')
         AND name = '{columnName}'
)
BEGIN
    ALTER TABLE {fullTableName}
    ADD [{columnName}] nvarchar(max)
END", cancellationToken);

#pragma warning restore EF1002
    }

    private static async Task<bool> DoesTableExist(DbContext context,
        string schemaName,
        string tableName,
        CancellationToken cancellationToken)
    {
        var conn = context.Database.GetDbConnection();

        if (conn.State == ConnectionState.Closed)
        {
            await conn.OpenAsync(cancellationToken);
        }

        await using var command = conn.CreateCommand();

        command.CommandText = $@"
    SELECT 1 FROM sys.tables AS T
        INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
    WHERE S.Name = '{schemaName}' AND T.Name = '{tableName}'";

        var exists = await command.ExecuteScalarAsync(cancellationToken) != null;

        return exists;
    }
}
