using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.HostedServices;

public class ConfigureCollectionsHostedService : BackgroundService
{
    private readonly ILogger<ConfigureCollectionsHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ConfigureCollectionsHostedService(IServiceProvider serviceProvider, ILogger<ConfigureCollectionsHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        await ConfigureCollectionsAsync(scope.ServiceProvider, _logger, stoppingToken);
    }

    private static async Task ConfigureCollectionsAsync(IServiceProvider serviceProvider,
        ILogger<ConfigureCollectionsHostedService> logger,
        CancellationToken stoppingToken)
    {
        var collections = serviceProvider.GetService<IEnumerable<IConfigureCollection>>();

        if (collections != null)
        {
            ConfigureCollectionsHostedServiceLogger.Start(logger);

            var configureTasks = collections.Select(x => x.ConfigureAsync(stoppingToken));

            await Task.WhenAll(configureTasks);

            ConfigureCollectionsHostedServiceLogger.Finish(logger);
        }
        else
        {
            logger.LogError("No Collections to Configure, but Mongo still registered");
        }
    }
}
