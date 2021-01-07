using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Configuration
{
    public abstract class MongoConfigureCollection<TKey, TEntity> : BaseMongoConfigureCollection<TKey, TEntity>,  IConfigureCollection<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        private readonly IMongoEntityConfiguration<TKey, TEntity> _configuration;

        protected MongoConfigureCollection(ILogger<MongoConfigureCollection<TKey, TEntity>> logger,
            IMongoIndexClient<TKey, TEntity> indexClient,
            IMongoEntityConfiguration<TKey, TEntity> configuration) :base(logger, indexClient)
        {
            _configuration = configuration;
        }

        public virtual Task ConfigureAsync(CancellationToken cancellationToken)
            => ConfigureAsync(_configuration, cancellationToken);
    }
}
