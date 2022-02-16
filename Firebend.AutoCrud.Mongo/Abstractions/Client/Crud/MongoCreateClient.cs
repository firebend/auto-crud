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
            IDomainEventContextProvider domainEventContextProvider,
            IMongoRetryService mongoRetryService) : base(client, logger, entityConfiguration, mongoRetryService)
        {
            _eventPublisher = eventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        protected virtual async Task<TEntity> CreateInternalAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();

            if (entity is IModifiedEntity modified)
            {
                var now = DateTimeOffset.Now;
                modified.CreatedDate = now;
                modified.ModifiedDate = now;
            }

            if (transaction != null)
            {
                var session = UnwrapSession(transaction);

                await RetryErrorAsync(() => mongoCollection.InsertOneAsync(session, entity, null, cancellationToken))
                    .ConfigureAwait(false);
            }
            else
            {
                await RetryErrorAsync(() => mongoCollection.InsertOneAsync(entity, null, cancellationToken))
                    .ConfigureAwait(false);
            }

            await PublishDomainEventAsync(entity, transaction, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        public virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => CreateInternalAsync(entity, null, cancellationToken);

        public Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
            => CreateInternalAsync(entity, entityTransaction, cancellationToken);

        protected virtual Task PublishDomainEventAsync(TEntity savedEntity, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
        {
            if (_eventPublisher == null || _eventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = savedEntity, EventContext = _domainEventContextProvider?.GetContext() };

            return _eventPublisher.PublishEntityAddEventAsync(domainEvent, entityTransaction, cancellationToken);
        }
    }
}
