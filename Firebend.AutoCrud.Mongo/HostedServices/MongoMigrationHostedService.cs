using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.HostedServices;

public class MongoMigrationHostedService : BackgroundService
{
    private readonly ILogger<MongoMigrationHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MongoMigrationHostedService(ILogger<MongoMigrationHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        await DoMigrationAsync(scope.ServiceProvider, _logger, stoppingToken);
    }

    private static async Task DoMigrationAsync(IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken)
    {
        var databaseSelector = serviceProvider.GetService<IMongoDefaultDatabaseSelector>();
        var dbName = databaseSelector?.DefaultDb;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            return;
        }

        var _mongoMigrationConnectionStringProvider = serviceProvider.GetService<IMongoMigrationConnectionStringProvider>();
        var connectionString = _mongoMigrationConnectionStringProvider.GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

        var client = new MongoClient(mongoClientSettings);

        var db = client.GetDatabase(dbName);

        var collection = db.GetCollection<MongoDbMigrationVersion>($"__{nameof(MongoDbMigrationVersion)}");

        var maxVersion = await collection.AsQueryable()
            .Select(x => x.Version)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);

        var migrations = serviceProvider.GetServices<IMongoMigration>();

        foreach (var migration in migrations
                     .Where(x => x.Version.Version > maxVersion)
                     .OrderBy(x => x.Version.Version))
        {
            try
            {
                await migration.ApplyMigrationAsync(cancellationToken);

                await collection.InsertOneAsync(migration.Version, new InsertOneOptions(), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error Applying mongo Migrations {Name}, {Version}",
                    migration.Version.Name,
                    migration.Version.Version);

                break;
            }
        }
    }
}
