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
        where TEntity : class, IEntity<TKey>
    {

        private readonly IDomainEventPublisherService<TKey, TEntity> _publisherService;

        protected MongoDeleteClient(IMongoClientFactory<TKey, TEntity> clientFactory,
            ILogger<MongoDeleteClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoRetryService mongoRetryService,
            IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(clientFactory, logger, entityConfiguration, mongoRetryService)
        {
            _publisherService = publisherService;
        }

        public async Task<TEntity> DeleteInternalAsync(Expression<Func<TEntity, bool>> filter,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            filter = await BuildFiltersAsync(filter, cancellationToken);

            var mongoCollection = await GetCollectionAsync();


            TEntity result;

            if (transaction != null)
            {
                var session = UnwrapSession(transaction);

                result = await RetryErrorAsync(() => mongoCollection.FindOneAndDeleteAsync(session, filter, null, cancellationToken))
                    .ConfigureAwait(false);
            }
            else
            {
                result = await RetryErrorAsync(() => mongoCollection.FindOneAndDeleteAsync(filter, null, cancellationToken))
                    .ConfigureAwait(false);
            }

            if (result is not null && _publisherService is not null)
            {
                await _publisherService.PublishDeleteEventAsync(result, transaction, cancellationToken);
            }

            return result;
        }

        public Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
            CancellationToken cancellationToken = default)
            => DeleteInternalAsync(filter, null, cancellationToken);

        public Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => DeleteInternalAsync(filter, entityTransaction, cancellationToken);
    }
}
