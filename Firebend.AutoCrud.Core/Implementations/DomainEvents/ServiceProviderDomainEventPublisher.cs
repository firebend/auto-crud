using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Implementations.DomainEvents
{
    public class ServiceProviderDomainEventPublisher : IEntityDomainEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderDomainEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            Task PublishAsync(CancellationToken token) =>
                ExecuteSubscribers<IEntityAddedDomainEventSubscriber<TEntity>, EntityAddedDomainEvent<TEntity>>(
                    domainEvent,
                (subscriber, de, ct) => subscriber.EntityAddedAsync(de, ct),
                    cancellationToken);

            return entityTransaction == null ?
                PublishAsync(cancellationToken) :
                entityTransaction.AddFunctionEnrollmentAsync(PublishAsync, cancellationToken);
        }

        public Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            Task PublishAsync(CancellationToken token) =>
                ExecuteSubscribers<IEntityDeletedDomainEventSubscriber<TEntity>, EntityDeletedDomainEvent<TEntity>>(
                    domainEvent,
                    (subscriber, de, ct) => subscriber.EntityDeletedAsync(de, ct),
                    cancellationToken);

            return entityTransaction == null ?
                PublishAsync(cancellationToken) :
                entityTransaction.AddFunctionEnrollmentAsync(PublishAsync, cancellationToken);
        }

        public Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            Task PublishAsync(CancellationToken token) =>
                ExecuteSubscribers<IEntityUpdatedDomainEventSubscriber<TEntity>, EntityUpdatedDomainEvent<TEntity>>(
                    domainEvent,
                    (subscriber, de, ct) => subscriber.EntityUpdatedAsync(de, ct),
                    cancellationToken);

            return entityTransaction == null ?
                PublishAsync(cancellationToken) :
                entityTransaction.AddFunctionEnrollmentAsync(PublishAsync, cancellationToken);
        }

        private async Task ExecuteSubscribers<TSubscriber, TEvent>(
            TEvent domainEvent,
            Func<TSubscriber, TEvent, CancellationToken, Task> func,
            CancellationToken cancellationToken)
            where TSubscriber : IDisposable
        {
            using var scope = _serviceProvider.CreateScope();

            var subscribers = scope
                .ServiceProvider
                .GetServices<TSubscriber>();

            var subscribersArray = subscribers as TSubscriber[] ?? subscribers.ToArray();

            var tasks = subscribersArray
                .Where(x => x != null)
                .Select(async x =>
                {
                    try
                    {
                        await func(x, domainEvent, cancellationToken).ConfigureAwait(false);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        return ex;
                    }
                });

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);

            foreach (var subscriber in subscribersArray)
            {
                try
                {
                    subscriber.Dispose();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {

                }
            }
        }
    }
}
