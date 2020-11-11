using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class DefaultDbCreator : IDbCreator
    {
        private readonly ShardMapMangerConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultDbCreator(ShardMapMangerConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            var creator = rootConnectionString.Contains("database.windows.net")
                ? (IDbCreator)new ElasticPoolDbCreator(_loggerFactory.CreateLogger<ElasticPoolDbCreator>(), _configuration)
                : new SqlServerDbCreator(_loggerFactory.CreateLogger<SqlServerDbCreator>());

            return creator.EnsureCreatedAsync(rootConnectionString, dbName, cancellationToken);
        }
    }
}
