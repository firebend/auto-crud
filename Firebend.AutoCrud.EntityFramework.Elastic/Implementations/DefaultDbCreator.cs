using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class DefaultDbCreator : IDbCreator
{
    private readonly ShardMapMangerConfiguration _configuration;
    private readonly ILogger<DefaultDbCreator> _logger;

    public DefaultDbCreator(ShardMapMangerConfiguration configuration, ILogger<DefaultDbCreator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task EnsureCreatedAsync(string rootConnectionString, string dbName, CancellationToken cancellationToken = default)
    {
        var creator = rootConnectionString.Contains("database.windows.net")
            ? (IDbCreator)new ElasticPoolDbCreator(_logger, _configuration)
            : new SqlServerDbCreator(_logger);

        return creator.EnsureCreatedAsync(rootConnectionString, dbName, cancellationToken);
    }
}
