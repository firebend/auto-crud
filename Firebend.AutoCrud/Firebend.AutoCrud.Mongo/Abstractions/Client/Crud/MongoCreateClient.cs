using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IEntityDomainEventPublisher _eventPublisher;

        protected MongoCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityDomainEventPublisher eventPublisher) : base(client, logger, entityConfiguration)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();

            await RetryErrorAsync(() => mongoCollection.InsertOneAsync(entity, null, cancellationToken))
                .ConfigureAwait(false);
            
            await _eventPublisher.PublishEntityAddEventAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            return entity;
        }
    }
}