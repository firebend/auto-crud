using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Data.SqlClient;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public abstract class AbstractDbCreator : IDbCreator
    {
        protected abstract string GetSqlCommand(string dbName);
        
        public async Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            await using var conn = await OpenConnection(null, rootConnectionString, cancellationToken).ConfigureAwait(false);
            await using var command = conn.CreateCommand();
            command.CommandText = GetSqlCommand(dbName);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        
        
        private static async Task<DbConnection> OpenConnection(string dbName, string connectionString, CancellationToken cancellationToken)
        {
            var connBuilder = new SqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrWhiteSpace(dbName))
            {
                connBuilder.InitialCatalog = dbName;
            }

            var cString = connBuilder.ConnectionString;

            var conn = new SqlConnection(cString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            return conn;
        }
    }
}