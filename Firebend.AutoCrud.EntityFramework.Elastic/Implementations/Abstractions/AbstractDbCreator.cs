using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions
{
    public abstract class AbstractDbCreator : IDbCreator
    {
        private readonly ILogger _logger;

        protected AbstractDbCreator(ILogger logger)
        {
            _logger = logger;
        }

        public async Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            var connBuilder = new SqlConnectionStringBuilder(rootConnectionString);

            _logger.CreatingDb(dbName, connBuilder.DataSource);

            var cString = connBuilder.ConnectionString;
            await using var conn = new SqlConnection(cString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = conn.CreateCommand();
            command.CommandText = GetSqlCommand(dbName);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _logger.DbCreated(dbName, connBuilder.DataSource);

            connBuilder = null;
        }

        protected abstract string GetSqlCommand(string dbName);
    }
}
