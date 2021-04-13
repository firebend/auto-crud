using System;
using System.Linq.Expressions;
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
    public abstract class MongoDeleteClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _entityDomainEventPublisher;

        protected MongoDeleteClient(IMongoClient client,
            ILogger<MongoDeleteClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IEntityDomainEventPublisher entityDomainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(client, logger, entityConfiguration)
        {
            _entityDomainEventPublisher = entityDomainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public async Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            filter = await BuildFiltersAsync(filter, cancellationToken);

            var mongoCollection = GetCollection();

            var result = await RetryErrorAsync(() => mongoCollection.FindOneAndDeleteAsync(filter, null, cancellationToken))
                .ConfigureAwait(false);

            if (result != null)
            {
                await PublishDomainEventAsync(result, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException(); //todo

        private Task PublishDomainEventAsync(TEntity savedEntity, CancellationToken cancellationToken = default)
        {
            if (_entityDomainEventPublisher == null || _entityDomainEventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityDeletedDomainEvent<TEntity> { Entity = savedEntity, EventContext = _domainEventContextProvider?.GetContext() };

            return _entityDomainEventPublisher.PublishEntityDeleteEventAsync(domainEvent, cancellationToken);
        }
    }
}
