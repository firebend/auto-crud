using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public class MongoConfigureShardedCollection<TKey, TEntity> : BaseMongoConfigureCollection<TKey, TEntity>,  IConfigureCollection<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IMongoAllShardsProvider _allShardsProvider;
        private readonly IMongoEntityDefaultConfiguration<TKey, TEntity> _defaultConfiguration;
        private readonly IMongoEntityConfigurationTenantTransformService<TKey, TEntity> _transformService;

        public MongoConfigureShardedCollection(ILogger<MongoConfigureShardedCollection<TKey,TEntity>> logger,
            IMongoIndexClient<TKey, TEntity> indexClient,
            IMongoAllShardsProvider allShardsProvider,
            IMongoEntityDefaultConfiguration<TKey, TEntity> defaultConfiguration,
            IMongoEntityConfigurationTenantTransformService<TKey, TEntity> transformService) : base(logger, indexClient)
        {
            _allShardsProvider = allShardsProvider;
            _defaultConfiguration = defaultConfiguration;
            _transformService = transformService;
        }

        public async Task ConfigureAsync(CancellationToken cancellationToken)
        {
            var shards = await _allShardsProvider.GetAllShardsAsync(cancellationToken);

            var configureTasks = shards
                .Select(x => new MongoEntityConfiguration<TKey, TEntity>(
                        _transformService.GetCollection(_defaultConfiguration, x),
                        _transformService.GetDatabase(_defaultConfiguration, x),
                        _defaultConfiguration.ShardMode))
                .Select(x => ConfigureAsync(x, cancellationToken))
                .ToArray();

            await Task.WhenAll(configureTasks);
        }
    }
}
