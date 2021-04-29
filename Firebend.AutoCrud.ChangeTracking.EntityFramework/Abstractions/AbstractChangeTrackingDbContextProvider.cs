using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    internal static class ChangeTrackingCaches
    {
        public static readonly ConcurrentDictionary<string, Task<bool>> InitCaches = new();
    }

    public abstract class AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity> :
        IChangeTrackingDbContextProvider<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
    {
        private readonly ILogger _logger;
        private readonly IDbContextOptionsProvider<TEntityKey, TEntity> _optionsProvider;
        private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _connectionStringProvider;

        protected AbstractChangeTrackingDbContextProvider(ILogger<AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity>> logger,
            IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
            IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider)
        {
            _logger = logger;
            _optionsProvider = optionsProvider;
            _connectionStringProvider = connectionStringProvider;
        }

        private async Task<IDbContext> GetDbContextAsync(DbContextOptions options, CancellationToken cancellationToken)
        {
            var context = new ChangeTrackingDbContext<TEntityKey, TEntity>(options);

            await ChangeTrackingCaches.InitCaches.GetOrAdd(typeof(TEntity).FullName ?? string.Empty, async _ =>
                {

                    var type =context.Model.FindEntityType(typeof(ChangeTrackingEntity<TEntityKey, TEntity>));
                    var schema = type.GetSchema().Coalesce("dbo");
                    var table = type.GetTableName();
                    var fullTableName = $"[{schema}].[{table}]";

                    try
                    {
                        if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator dbCreator)
                        {
                            var exists = await DoesTableExist(context, schema, table, cancellationToken);

                            if (!exists)
                            {
                                await dbCreator
                                    .CreateTablesAsync(cancellationToken)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.StartsWith("There is already an object named"))
                        {
                            _logger.LogError(ex, "Error creating change tracking tables for context");
                        }
                    }

                    await AddMigrationFieldsAsync(context, fullTableName, cancellationToken);

                    return true;
                })
                .ConfigureAwait(false);

            return context;
        }

        private static async Task AddMigrationFieldsAsync(DbContext context,
            string fullTableName,
            CancellationToken cancellationToken)
        {
            //todo: refactor this later to be more robust and not depend on sql server syntax
            const string columnName = nameof(ChangeTrackingEntity<TEntityKey, TEntity>.DomainEventCustomContext);
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

        public async Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
        {
            var connectionString = await _connectionStringProvider
                .GetConnectionStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var options = _optionsProvider.GetDbContextOptions(connectionString);

            var context = await GetDbContextAsync(options, cancellationToken);

            return context;
        }

        public async Task<IDbContext> GetDbContextAsync(DbConnection connection, CancellationToken cancellationToken = default)
        {
            var options = _optionsProvider.GetDbContextOptions(connection);

            var context = await GetDbContextAsync(options, cancellationToken);

            return context;
        }
    }
}
