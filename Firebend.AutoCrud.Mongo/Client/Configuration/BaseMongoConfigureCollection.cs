using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Client.Configuration;

public class BaseMongoConfigureCollection<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    private readonly ILogger _logger;
    private readonly IMongoIndexClient<TKey, TEntity> _indexClient;

    public BaseMongoConfigureCollection(ILogger logger, IMongoIndexClient<TKey, TEntity> indexClient)
    {
        _logger = logger;
        _indexClient = indexClient;
    }

    protected virtual async Task ConfigureAsync(IMongoEntityIndexConfiguration<TKey, TEntity> configuration,
        CancellationToken cancellationToken)
    {
        BaseMongoConfigureCollectionLogger.ConfiguringCollection(_logger, configuration.DatabaseName, configuration.CollectionName);
        await _indexClient.CreateCollectionAsync(configuration, cancellationToken);

        BaseMongoConfigureCollectionLogger.ConfiguringIndexes(_logger, configuration.DatabaseName, configuration.CollectionName);
        await _indexClient.BuildIndexesAsync(configuration, cancellationToken);

        BaseMongoConfigureCollectionLogger.Done(_logger, configuration.DatabaseName, configuration.CollectionName);
    }
}
