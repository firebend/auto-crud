using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public class MongoConfigureShardedCollection<TKey, TEntity> : BaseMongoConfigureCollection<TKey, TEntity>, IConfigureCollection<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IMongoConfigurationAllShardsProvider<TKey, TEntity> _configurationAllShardsProvider;

        public MongoConfigureShardedCollection(ILogger<MongoConfigureShardedCollection<TKey, TEntity>> logger,
            IMongoIndexClient<TKey, TEntity> indexClient,
            IMongoConfigurationAllShardsProvider<TKey, TEntity> configurationAllShardsProvider) : base(logger, indexClient)
        {
            _configurationAllShardsProvider = configurationAllShardsProvider;
        }

        public async Task ConfigureAsync(CancellationToken cancellationToken)
        {
            var configurations = await _configurationAllShardsProvider
                .GetAllEntityConfigurationsAsync(cancellationToken);

            foreach (var configuration in configurations)
            {
                await ConfigureAsync(configuration, cancellationToken);
            }
        }
    }
}
