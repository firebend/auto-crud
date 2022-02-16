using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public class MongoClientFactory : IMongoClientFactory
    {
        private readonly ILogger _logger;

        public MongoClientFactory(ILogger<MongoClientFactory> logger)
        {
            _logger = logger;
        }

        public IMongoClient CreateClient(string connectionString, bool enableLogging)
        {
            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

            if (enableLogging)
            {
                mongoClientSettings.ClusterConfigurator = Configurator;
            }

            return new MongoClient(mongoClientSettings);
        }

        private void Configurator(ClusterBuilder cb)
        {
            cb.Subscribe<CommandStartedEvent>(e => MongoClientFactoryLogger.Started(_logger, e.CommandName, e.Command));

            cb.Subscribe<CommandSucceededEvent>(e => MongoClientFactoryLogger.Success(_logger, e.CommandName, e.Duration, e.Reply));

            cb.Subscribe<CommandFailedEvent>(e => MongoClientFactoryLogger.Failed(_logger, e.CommandName, e.Duration));
        }
    }
}
