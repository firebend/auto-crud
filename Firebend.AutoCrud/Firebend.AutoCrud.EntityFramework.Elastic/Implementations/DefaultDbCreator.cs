using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations
{
    public class DefaultDbCreator : IDbCreator
    {
        private readonly ShardMapMangerConfiguration _configuration;

        public DefaultDbCreator(ShardMapMangerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
        {
            var  creator = rootConnectionString.Contains("database.windows.net")
                ? (IDbCreator) new ElasticPoolDbCreator(_configuration)
                : new SqlServerDbCreator();

            return creator.EnsureCreatedAsync(rootConnectionString, dbName, cancellationToken);
        }
    }
}