using System;
using System.Collections.Generic;
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

namespace Firebend.AutoCrud.Mongo.HostedServices
{
    public class MongoMigrationHostedService : BackgroundService
    {
        private readonly IEnumerable<IMongoMigration> _migrations;
        private readonly IMongoDefaultDatabaseSelector _databaseSelector;
        private readonly IMongoMigrationConnectionStringProvider _mongoMigrationConnectionStringProvider;
        private readonly ILogger _logger;

        public MongoMigrationHostedService(ILogger<MongoMigrationHostedService> logger, IServiceProvider serviceProvider, IMongoMigrationConnectionStringProvider mongoMigrationConnectionStringProvider)
        {
            _mongoMigrationConnectionStringProvider = mongoMigrationConnectionStringProvider;
            _logger = logger;

            using var scope = serviceProvider.CreateScope();

            _migrations = scope.ServiceProvider.GetService<IEnumerable<IMongoMigration>>();
            _databaseSelector = scope.ServiceProvider.GetService<IMongoDefaultDatabaseSelector>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => DoMigration(stoppingToken);

        private async Task DoMigration(CancellationToken cancellationToken)
        {
            var dbName = _databaseSelector?.DefaultDb;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return;
            }

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
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var migration in _migrations
                .Where(x => x.Version.Version > maxVersion)
                .OrderBy(x => x.Version.Version))
            {
                try
                {
                    await migration
                        .ApplyMigrationAsync(cancellationToken)
                        .ConfigureAwait(false);

                    await collection
                        .InsertOneAsync(migration.Version, new InsertOneOptions(), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error Applying mongo Migrations {Name}, {Version}",
                        migration.Version.Name,
                        migration.Version.Version);

                    break;
                }
            }
        }
    }
}
