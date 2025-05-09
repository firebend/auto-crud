using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Firebend.AutoCrud.Mongo.Client;

public class MongoClientFactory<TKey, TEntity> : IMongoClientFactory<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private record MongoClientCacheFactoryContext(ILoggerFactory LoggerFactory,
        MongoClientSettings Settings,
        IMongoClientSettingsConfigurator SettingsConfigurator);

    private readonly IMongoConnectionStringProvider<TKey, TEntity> _connectionStringProvider;
    private readonly IMongoClientSettingsConfigurator _settingsConfigurator;
    private readonly ILoggerFactory _loggerFactory;

    public MongoClientFactory(IMongoConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        ILoggerFactory loggerFactory,
        IMongoClientSettingsConfigurator settingsConfigurator = null)
    {
        _connectionStringProvider = connectionStringProvider;
        _loggerFactory = loggerFactory;
        _settingsConfigurator = settingsConfigurator;
    }

    public async Task<IMongoClient> CreateClientAsync(string overrideShardKey, CancellationToken cancellationToken)
    {
        var connectionString = await _connectionStringProvider.GetConnectionStringAsync(overrideShardKey, cancellationToken);

        var mongoClientSettings = MongoClientSettings.FromConnectionString(connectionString);

        var client = MongoClientFactoryCache.MongoClients.GetOrAdd(
            mongoClientSettings.Server.ToString(),
            CreateClientForCache,
            new MongoClientCacheFactoryContext(_loggerFactory, mongoClientSettings, _settingsConfigurator)
        );

        return client;
    }

    private static IMongoClient CreateClientForCache(string server, MongoClientCacheFactoryContext context)
    {
        context.Settings.LoggingSettings ??= new LoggingSettings(context.LoggerFactory);

        var settings = context.SettingsConfigurator is null
            ? context.Settings
            : context.SettingsConfigurator.Configure(server, context.Settings);

        return new MongoClient(settings);
    }
}
