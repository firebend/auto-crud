using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoCreateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {

        private readonly IDomainEventPublisherService<TKey, TEntity> _publisherService;

        protected MongoCreateClient(IMongoClientFactory<TKey, TEntity> clientFactory,
            ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoRetryService mongoRetryService,
            IDomainEventPublisherService<TKey, TEntity> publisherService)
            : base(clientFactory, logger, entityConfiguration, mongoRetryService)
        {
            _publisherService = publisherService;
        }

        protected virtual async Task<TEntity> CreateInternalAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken = default)
        {
            var mongoCollection = await GetCollectionAsync();

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

            return await _publisherService.ReadAndPublishAddedEventAsync(entity.Id, transaction, cancellationToken);
        }

        public virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
            => CreateInternalAsync(entity, null, cancellationToken);

        public Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
            => CreateInternalAsync(entity, entityTransaction, cancellationToken);
    }
}
