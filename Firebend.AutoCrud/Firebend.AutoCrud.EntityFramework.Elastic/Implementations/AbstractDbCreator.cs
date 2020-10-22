using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public abstract class AbstractDbCreator : IDbCreator
    {
        private readonly ILogger _logger;

        protected AbstractDbCreator(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract string GetSqlCommand(string dbName);
        
        public Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            var connBuilder = new SqlConnectionStringBuilder(rootConnectionString);

            var key = $"{connBuilder.DataSource}-{dbName}";
            
            var runKey = $"{GetType().FullName}.{key}";

            return Run.OnceAsync(runKey, async ct =>
            {
                _logger.LogDebug($"Creating database. Key {key}");

                var cString = connBuilder.ConnectionString;
                await using var conn = new SqlConnection(cString);
                await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using var command = conn.CreateCommand();
                command.CommandText = GetSqlCommand(dbName);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogDebug($"Database is created. Key {key}");
            }, cancellationToken);
        }
    }
}