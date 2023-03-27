using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public class MongoClientFactory<TKey, TEntity> : IMongoClientFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly ILogger _logger;
        private readonly IMongoConnectionStringProvider<TKey, TEntity> _connectionStringProvider;

        public MongoClientFactory(ILogger<MongoClientFactory<TKey, TEntity>> logger,
            IMongoConnectionStringProvider<TKey, TEntity> connectionStringProvider)
        {
            _logger = logger;
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<IMongoClient> CreateClientAsync(string overrideShardKey = null, bool enableLogging = false)
        {
            var connectionString = await _connectionStringProvider.GetConnectionStringAsync(overrideShardKey);

            var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);
            //********************************************
            // Author: JMA
            // Date: 2023-03-27 02:04:09
            // Comment: Mongo is planning on a version three of their driver.
            // When using the V3 of the linq provider there are issues with having expressions that use
            // object. For example: the AbstractEntitySearchService's GetSearchExpression function
            //*******************************************
            mongoClientSettings.LinqProvider = LinqProvider.V2;

            if (enableLogging)
            {
                mongoClientSettings.ClusterConfigurator = Configurator;
            }

            return new MongoClient(mongoClientSettings);
        }

        protected virtual void Configurator(ClusterBuilder cb)
        {
            cb.Subscribe<CommandStartedEvent>(e => MongoClientFactoryLogger.Started(_logger, e.CommandName, e.Command));

            cb.Subscribe<CommandSucceededEvent>(e => MongoClientFactoryLogger.Success(_logger, e.CommandName, e.Duration, e.Reply));

            cb.Subscribe<CommandFailedEvent>(e => MongoClientFactoryLogger.Failed(_logger, e.CommandName, e.Duration));
        }
    }
}
