using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityTableCreator : IEntityTableCreator
    {
        public async Task<bool> EnsureExistsAsync<TEntity>(IDbContext dbContext, CancellationToken cancellationToken)
        {
            var schema = dbContext.Model.GetSchemaName<TEntity>();
            var table = dbContext.Model.GetTableName<TEntity>();

            var exists = await DoesTableExist(dbContext, schema, table, cancellationToken).ConfigureAwait(false);

            if (exists)
            {
                return false;
            }

            var created = await CreateTableAsync(dbContext,table, cancellationToken);

            return created;
        }

        private static async Task<bool> CreateTableAsync(IDbContext dbContext, string tableName, CancellationToken cancellationToken)
        {
            var script = dbContext.Database.GenerateCreateScript();

            var split = script.Split(new[] { "CREATE TABLE " }, StringSplitOptions.RemoveEmptyEntries);
            string commandText = null;

            foreach (var sql in split)
            {
                var scriptTableName = sql.Substring(0, sql.IndexOf("(", StringComparison.OrdinalIgnoreCase));
                scriptTableName = scriptTableName.Split('.').Last();
                scriptTableName = scriptTableName.Trim().TrimStart('[').TrimEnd(']').ToLowerInvariant();

                if (!scriptTableName.EqualsIgnoreCaseAndWhitespace(tableName))
                {
                    continue;
                }

                commandText =  "CREATE TABLE " + sql.Substring(0, sql.LastIndexOf(";"));
                break;
            }

            if (string.IsNullOrWhiteSpace(commandText))
            {
                return false;
            }

            await using var connection = await GetDbConnection(dbContext, cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }

        private static async Task<DbConnection> GetDbConnection(IDbContext context, CancellationToken cancellationToken)
        {
            var connection = context.Database.GetDbConnection();

            bool isConnectionClosed = connection.State == ConnectionState.Closed;

            if (isConnectionClosed)
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return connection;
        }

        private static async Task<bool> DoesTableExist(
            IDbContext dbContext,
            string schema,
            string table,
            CancellationToken cancellationToken)
        {
            await using var connection = await GetDbConnection(dbContext, cancellationToken).ConfigureAwait(false);

            await using var command = connection.CreateCommand();

            AddStringParameter("TableName", table, command);
            AddStringParameter("SchemaName", schema, command);
            AddStringParameter("TableType", "base table", command);
            SetCommandText(command);

            var existsObj = await command.ExecuteScalarAsync(cancellationToken);

            if (existsObj == null)
            {
                return false;
            }

            var existsBool = (bool)existsObj;
            return existsBool;
        }

        private static void AddStringParameter(string name, string value, DbCommand command)
        {
            var parameter = command.CreateParameter();
            parameter.Value = (object) value ?? DBNull.Value;
            parameter.ParameterName = name;
            parameter.DbType = DbType.String;
            command.Parameters.Add(parameter);
        }

        private static void SetCommandText(IDbCommand command) => command.CommandText = @"
IF (EXISTS ( SELECT *
    FROM
    [INFORMATION_SCHEMA].[TABLES] [Tables]
    WHERE [Tables].[TABLE_TYPE] = @TableType
    AND [Tables].TABLE_NAME = @TableName
    AND ([Tables].[TABLE_SCHEMA] = @SchemaName OR @SchemaName IS NULL OR @SchemaName = '')
))
SELECT CAST(1 as BIT)
ELSE
SELECT CAST(0 as BIT)";
    }
}
