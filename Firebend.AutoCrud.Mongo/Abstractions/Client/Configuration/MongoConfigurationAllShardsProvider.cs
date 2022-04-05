using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Implementations;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public abstract class MongoConfigurationAllShardsProvider<TKey, TEntity> : IMongoConfigurationAllShardsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IMongoAllShardsProvider _allShardsProvider;
        private readonly IMongoEntityDefaultConfiguration<TKey, TEntity> _defaultConfiguration;
        private readonly IMongoEntityConfigurationTenantTransformService<TKey, TEntity> _transformService;

        protected MongoConfigurationAllShardsProvider(IMongoAllShardsProvider allShardsProvider,
            IMongoEntityDefaultConfiguration<TKey, TEntity> defaultConfiguration,
            IMongoEntityConfigurationTenantTransformService<TKey, TEntity> transformService)
        {
            _allShardsProvider = allShardsProvider;
            _defaultConfiguration = defaultConfiguration;
            _transformService = transformService;
        }

        public async Task<IEnumerable<IMongoEntityConfiguration<TKey, TEntity>>> GetAllEntityConfigurationsAsync(CancellationToken cancellationToken)
        {
            var shards = await _allShardsProvider.GetAllShardsAsync(cancellationToken);

            var configurations = shards
                .Select(x => new MongoEntityConfiguration<TKey, TEntity>(
                    _transformService.GetCollection(_defaultConfiguration, x),
                    _transformService.GetDatabase(_defaultConfiguration, x),
                    _defaultConfiguration.AggregateOption,
                    _defaultConfiguration.ShardMode))
                .ToArray();

            return configurations;
        }
    }
}
