using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Threading;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    internal static class CreatedDatabases
    {
        public static readonly ConcurrentDictionary<string, bool> CreatedCache = new ConcurrentDictionary<string, bool>();

        public static bool IsCreated(string key)
        {
            return CreatedCache[key];
        }

        public static void  MarkAsCreate(string key)
        {
            CreatedCache.AddOrUpdate(key,
                true,
                (s, b) => true);
        }
    }
    
    public abstract class AbstractDbCreator : IDbCreator
    {
        private readonly ILogger _logger;

        protected AbstractDbCreator(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract string GetSqlCommand(string dbName);
        
        public async Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            var connBuilder = new SqlConnectionStringBuilder(rootConnectionString);

            if (!string.IsNullOrWhiteSpace(dbName))
            {
                connBuilder.InitialCatalog = dbName;
            }

            var key = $"{connBuilder.DataSource}-{dbName}";
            
            _logger.LogDebug($"Ensuring database is created. Key {key}");

            if (CreatedDatabases.IsCreated(key))
            {
                _logger.LogDebug($"Database is created. Key {key}");
                return;
            }

            using var _ = await new AsyncDuplicateLock().LockAsync(key, CancellationToken.None);

            if (CreatedDatabases.IsCreated(key))
            {
                _logger.LogDebug($"Database is created. Key {key}");
                return;
            }
            
            _logger.LogDebug($"Database is not created. Creating now.... Key {key}");

            var cString = connBuilder.ConnectionString;
            await using var conn = new SqlConnection(cString);
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            
            await using var command = conn.CreateCommand();
            command.CommandText = GetSqlCommand(dbName);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            
            CreatedDatabases.MarkAsCreate(key);
            
            _logger.LogDebug($"Database is created. Key {key}");
        }
    }
}