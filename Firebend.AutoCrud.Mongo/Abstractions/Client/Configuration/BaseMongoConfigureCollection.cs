using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public abstract class BaseMongoConfigureCollection<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly ILogger _logger;
        private readonly IMongoIndexClient<TKey, TEntity> _indexClient;

        protected BaseMongoConfigureCollection(ILogger logger, IMongoIndexClient<TKey, TEntity> indexClient)
        {
            _logger = logger;
            _indexClient = indexClient;
        }

        protected virtual async Task ConfigureAsync(IMongoEntityConfiguration<TKey, TEntity> configuration,
            CancellationToken cancellationToken)
        {
            var fullCollectionName = $"{configuration.DatabaseName}.{configuration.CollectionName}";

            _logger.LogDebug("Configuring collection for {Collection}", fullCollectionName);

            await _indexClient.CreateCollectionAsync(configuration, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Configuring indexes for {Collection}", fullCollectionName);

            await _indexClient.BuildIndexesAsync(configuration, cancellationToken).ConfigureAwait(false);
        }
    }
}
