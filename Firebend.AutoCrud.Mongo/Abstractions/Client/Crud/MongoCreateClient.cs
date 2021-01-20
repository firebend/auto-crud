using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _eventPublisher;

        protected MongoCreateClient(IMongoClient client,
            ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityDomainEventPublisher eventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(client, logger, entityConfiguration)
        {
            _eventPublisher = eventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();

            if (entity is IModifiedEntity modified)
            {
                var now = DateTimeOffset.Now;
                modified.CreatedDate = now;
                modified.ModifiedDate = now;
            }

            await RetryErrorAsync(() => mongoCollection.InsertOneAsync(entity, null, cancellationToken))
                .ConfigureAwait(false);

            await PublishDomainEventAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        private Task PublishDomainEventAsync(TEntity savedEntity, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher == null || _eventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = savedEntity, EventContext = _domainEventContextProvider?.GetContext() };

            return _eventPublisher.PublishEntityAddEventAsync(domainEvent, cancellationToken);

        }
    }
}
