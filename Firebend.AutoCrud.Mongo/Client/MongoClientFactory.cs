using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Client;

public class MongoClientFactory<TKey, TEntity> : IMongoClientFactory<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private record MongoClientCacheFactoryContext(ILogger Logger,
        MongoClientSettings Settings,
        bool EnableLogging,
        IMongoClientSettingsConfigurator SettingsConfigurator);

    private readonly ILogger _logger;
    private readonly IMongoConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IMongoClientSettingsConfigurator _settingsConfigurator;

    public MongoClientFactory(ILogger<MongoClientFactory<TKey, TEntity>> logger,
        IMongoConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        IMongoClientSettingsConfigurator settingsConfigurator = null)
    {
        _logger = logger;
        _connectionStringProvider = connectionStringProvider;
        _settingsConfigurator = settingsConfigurator;
    }

    public async Task<IMongoClient> CreateClientAsync(string overrideShardKey = null, bool enableLogging = false)
    {
        var connectionString = await _connectionStringProvider.GetConnectionStringAsync(overrideShardKey);

        var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

        var client = MongoClientFactoryCache.MongoClients.GetOrAdd(
            mongoClientSettings.Server.ToString(),
            CreateClientForCache,
            new MongoClientCacheFactoryContext(_logger, mongoClientSettings, enableLogging, _settingsConfigurator)
        );

        return client;
    }

    private static IMongoClient CreateClientForCache(string server, MongoClientCacheFactoryContext context)
    {
        context.Settings.LinqProvider = LinqProvider.V3;

        if (context.EnableLogging)
        {
            context.Settings.ClusterConfigurator = cb => Configurator(cb, context);
        }

        var settings = context.SettingsConfigurator is null
            ? context.Settings
            : context.SettingsConfigurator.Configure(server, context.Settings);

        return new MongoClient(settings);
    }

    private static void Configurator(ClusterBuilder cb, MongoClientCacheFactoryContext context)
    {
        cb.Subscribe<CommandStartedEvent>(e => MongoClientFactoryLogger.Started(context.Logger, e.CommandName, e.Command));

        cb.Subscribe<CommandSucceededEvent>(e => MongoClientFactoryLogger.Success(context.Logger, e.CommandName, e.Duration, e.Reply));

        cb.Subscribe<CommandFailedEvent>(e => MongoClientFactoryLogger.Failed(context.Logger, e.CommandName, e.Duration));
    }
}
