using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task PublishEntityAddEventAsync<TEntity>(EntityAddedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var tasks = GetSubscribers<IEntityAddedDomainEventSubscriber<TEntity>>()
                .Select(x => x.EntityAddedAsync(domainEvent, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task PublishEntityDeleteEventAsync<TEntity>(EntityDeletedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var tasks = GetSubscribers<IEntityDeletedDomainEventSubscriber<TEntity>>()
                .Select(x => x.EntityDeletedAsync(domainEvent, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task PublishEntityUpdatedEventAsync<TEntity>(EntityUpdatedDomainEvent<TEntity> domainEvent, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var tasks = GetSubscribers<IEntityUpdatedDomainEventSubscriber<TEntity>>()
                .Select(x => x.EntityUpdatedAsync(domainEvent, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private IEnumerable<T> GetSubscribers<T>()
        {
            using var scope = _serviceProvider.CreateScope();

            var services = scope
                .ServiceProvider
                .GetServices<T>() ?? Enumerable.Empty<T>();

            return services.Where(x => x != null);
        }
    }
}