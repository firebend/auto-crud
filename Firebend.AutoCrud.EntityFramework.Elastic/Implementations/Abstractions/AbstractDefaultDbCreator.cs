using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions
{
    public abstract class AbstractDefaultDbCreator : IDbCreator
    {
        private readonly ShardMapMangerConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        protected AbstractDefaultDbCreator(ShardMapMangerConfiguration configuration, ILoggerFactory loggerFactory)
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
