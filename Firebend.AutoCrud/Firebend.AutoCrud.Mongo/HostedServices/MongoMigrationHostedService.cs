#region

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

#endregion

namespace Firebend.AutoCrud.Mongo.HostedServices
{
    public class MongoMigrationHostedService : IHostedService
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IMongoDefaultDatabaseSelector _databaseSelector;
        private readonly ILogger _logger;
        private readonly IEnumerable<IMongoMigration> _migrations;
        private readonly IMongoClient _mongoClient;

        public MongoMigrationHostedService(ILogger<MongoMigrationHostedService> logger, IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            _migrations = scope.ServiceProvider.GetService<IEnumerable<IMongoMigration>>();
            _logger = logger;
            _databaseSelector = scope.ServiceProvider.GetService<IMongoDefaultDatabaseSelector>();
            _mongoClient = scope.ServiceProvider.GetService<IMongoClient>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return DoMigration(_cancellationTokenSource.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        private async Task DoMigration(CancellationToken cancellationToken)
        {
            var dbName = _databaseSelector?.DefaultDb;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return;
                throw new Exception("No default db name provided.");
            }

            var db = _mongoClient.GetDatabase(dbName);

            var collection = db.GetCollection<MongoDbMigrationVersion>($"__{nameof(MongoDbMigrationVersion)}");

            var maxVersion = await collection.AsQueryable()
                .Select(x => x.Version)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var migration in _migrations
                .Where(x => x.Version.Version > maxVersion)
                .OrderBy(x => x.Version.Version))
                try
                {
                    await migration.ApplyMigrationAsync(cancellationToken).ConfigureAwait(false);
                    await collection.InsertOneAsync(migration.Version, new InsertOneOptions(), cancellationToken)
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