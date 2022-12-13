using System;
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
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions
{
    public abstract class AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext> :
        IChangeTrackingDbContextProvider<TEntityKey, TEntity>
        where TEntity : class, IEntity<TEntityKey>
        where TEntityKey : struct
        where TContext : DbContext, IDbContext
    {
        private readonly IChangeTrackingOptionsProvider<TEntityKey, TEntity> _changeTrackingOptionsProvider;
        private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _connectionStringProvider;
        private readonly IDbContextOptionsProvider<TEntityKey, TEntity> _optionsProvider;
        private readonly IMemoizer _memoizer;

        protected AbstractChangeTrackingDbContextProvider(IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
            IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider,
            IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider,
            IMemoizer memoizer)
        {
            _optionsProvider = optionsProvider;
            _connectionStringProvider = connectionStringProvider;
            _changeTrackingOptionsProvider = changeTrackingOptionsProvider;
            _memoizer = memoizer;
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

            var key = GetScaffoldingKey(typeof(TEntity));

            await _memoizer.MemoizeAsync<bool, (
                AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext> self,
                ChangeTrackingDbContext<TEntityKey, TEntity> context,
                CancellationToken cancellationToken
                )>(
                key,
                static arg => arg.self.ScaffoldAsync(arg.context, arg.cancellationToken),
                (this, context, cancellationToken),
                cancellationToken);

            return context;
        }

        private string _scaffoldKey;

        protected virtual string GetScaffoldingKey(Type type) => _scaffoldKey ??= $"{type.FullName}.Changes.Scaffolding";

        private async Task<bool> ScaffoldAsync(DbContext context, CancellationToken cancellationToken)
        {
            var type = context.Model.FindEntityType(typeof(ChangeTrackingEntity<TEntityKey, TEntity>));

            if (type is null)
            {
                throw new Exception("Could not find entity type.");
            }

            var schema = type.GetSchema().Coalesce("dbo");
            var table = type.GetTableName();

            var fullTableName = $"[{schema}].[{table}]";

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

            if (_changeTrackingOptionsProvider?.Options?.PersistCustomContext ?? false)
            {
                await AddMigrationFieldsAsync(context, fullTableName, cancellationToken);
            }

            return true;
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
    }
}
