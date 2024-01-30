using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Client;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

public class ChangeTrackingDbContextProvider<TEntityKey, TEntity> :
    DbContextProvider<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingDbContext<TEntityKey, TEntity>>,
    IChangeTrackingDbContextProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    private readonly IChangeTrackingOptionsProvider<TEntityKey, TEntity> _changeTrackingOptionsProvider;
    private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _rootConnectionStringProvider;

    private record ScaffoldContext(DbContext DbContext, bool PersistCustomContext);

    public ChangeTrackingDbContextProvider(
        ILogger<ChangeTrackingDbContextProvider<TEntityKey, TEntity>> logger,
        IDbContextFactory<ChangeTrackingDbContext<TEntityKey, TEntity>> contextFactory,
        IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider = null,
        IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider = null) :
        base(logger, contextFactory)
    {
        _changeTrackingOptionsProvider = changeTrackingOptionsProvider;
        _rootConnectionStringProvider = connectionStringProvider;
    }

    protected override void InitDb(DbContext dbContext)
    {
        base.InitDb(dbContext);
        ScaffoldDbContext(_changeTrackingOptionsProvider?.Options, dbContext);
    }

    protected override async Task<string> ProvideConnectionString(CancellationToken cancellationToken)
    {
        if (_rootConnectionStringProvider is null)
        {
            return null;
        }

        return await _rootConnectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }

    private static void ScaffoldDbContext(ChangeTrackingOptions changeTrackingOptions, DbContext context)
        => ChangeTrackingDbContextProviderCache.ScaffoldCache.GetOrAdd(typeof(TEntity).FullName,
            ScaffoldCacheFactory,
            new ScaffoldContext(context, changeTrackingOptions?.PersistCustomContext ?? false)
        );

    private static bool ScaffoldCacheFactory(string typeName, ScaffoldContext context)
    {
        var type = context.DbContext.Model.FindEntityType(typeof(ChangeTrackingEntity<TEntityKey, TEntity>))
                   ?? throw new Exception("Could not find entity type.");

        var schema = type.GetSchema().Coalesce("dbo");
        var table = type.GetTableName();

        var fullTableName = $"[{schema}].[{table}]";

        if (context.DbContext.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
        {
            var exists = DoesTableExist(context.DbContext, schema, table);

            if (!exists)
            {
                dbCreator.CreateTables();
            }
        }

        if (context.PersistCustomContext)
        {
            AddMigrationFields(context.DbContext, fullTableName);
        }

        return true;
    }

    private static void AddMigrationFields(DbContext context, string fullTableName)
    {
        //todo: refactor this later to be more robust and not depend on sql server syntax
        const string columnName = nameof(ChangeTrackingEntity<TEntityKey, TEntity>.DomainEventCustomContext);

#pragma warning disable EF1002
        // ReSharper disable once UseRawString
        context.Database.ExecuteSqlRaw($@"
IF NOT EXISTS (
  SELECT *
  FROM   sys.columns
  WHERE  object_id = OBJECT_ID(N'{fullTableName}')
         AND name = '{columnName}'
)
BEGIN
    ALTER TABLE {fullTableName}
    ADD [{columnName}] nvarchar(max)
END");

#pragma warning restore EF1002
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

        var exists = command.ExecuteScalar() != null;

        return exists;
    }
}
