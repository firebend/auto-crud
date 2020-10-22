using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoDeleteClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
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
                var domainEvent = new EntityDeletedDomainEvent<TEntity>
                {
                    Entity = result
                };
                    
                await _entityDomainEventPublisher
                    .PublishEntityDeleteEventAsync(domainEvent, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
        }
    }
}