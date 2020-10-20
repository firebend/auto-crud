using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoDeleteClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityDomainEventPublisher _entityDomainEventPublisher;

        protected MongoDeleteClient(IMongoClient client,
            ILogger<MongoDeleteClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityDomainEventPublisher entityDomainEventPublisher) : base(client, logger, entityConfiguration)
        {
            _entityDomainEventPublisher = entityDomainEventPublisher;
        }

        public async Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            filter = BuildFilters(filter);

            var mongoCollection = GetCollection();

            var result = await RetryErrorAsync(() => mongoCollection.FindOneAndDeleteAsync(filter, null, cancellationToken))
                .ConfigureAwait(false);

            if (result != null)
            {
                await _entityDomainEventPublisher
                    .PublishEntityDeleteEventAsync(result, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }
    }
}