using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public abstract class MongoConfigureCollection<TKey, TEntity> : IConfigureCollection<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IMongoIndexClient<TKey, TEntity> _indexClient;
        private readonly ILogger _logger;

        protected MongoConfigureCollection(ILogger<MongoConfigureCollection<TKey, TEntity>> logger,
            IMongoIndexClient<TKey, TEntity> indexClient)
        {
            _logger = logger;
            _indexClient = indexClient;
        }

        public virtual async Task ConfigureAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Configuring collection for {Collection}", typeof(TEntity).FullName);

            await _indexClient.CreateCollectionAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Configuring indexes for {Collection}", typeof(TEntity).FullName);

            await _indexClient.BuildIndexesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}