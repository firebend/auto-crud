using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Implementations.DomainEvents;

public class ServiceProviderDomainEventPublisher<TKey, TEntity> : IEntityDomainEventPublisher<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ServiceProviderDomainEventPublisher(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task PublishEntityAddEventAsync(EntityAddedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        return entityTransaction == null
            ? PublishAsync(cancellationToken)
            : entityTransaction
                .AddFunctionEnrollmentAsync<TEntity, ServiceProviderDomainEventEntityTransactionOutboxEnrollment>(
                    PublishAsync, cancellationToken);

        Task PublishAsync(CancellationToken token)
        {
            return ExecuteSubscribers<IEntityAddedDomainEventSubscriber<TEntity>, EntityAddedDomainEvent<TEntity>>(
                domainEvent,
                (subscriber, de, ct) => subscriber.EntityAddedAsync(de, ct),
                cancellationToken);
        }
    }

    public Task PublishEntityDeleteEventAsync(EntityDeletedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        return entityTransaction == null
            ? PublishAsync(cancellationToken)
            : entityTransaction
                .AddFunctionEnrollmentAsync<TEntity, ServiceProviderDomainEventEntityTransactionOutboxEnrollment>(
                    PublishAsync, cancellationToken);

        Task PublishAsync(CancellationToken token)
        {
            return ExecuteSubscribers<IEntityDeletedDomainEventSubscriber<TEntity>, EntityDeletedDomainEvent<TEntity>>(
                domainEvent,
                (subscriber, de, ct) => subscriber.EntityDeletedAsync(de, ct),
                cancellationToken);
        }
    }

    public Task PublishEntityUpdatedEventAsync(EntityUpdatedDomainEvent<TEntity> domainEvent,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken = default)
    {
        return entityTransaction == null
            ? PublishAsync(cancellationToken)
            : entityTransaction
                .AddFunctionEnrollmentAsync<TEntity, ServiceProviderDomainEventEntityTransactionOutboxEnrollment>(
                    PublishAsync, cancellationToken);

        Task PublishAsync(CancellationToken token)
        {
            return ExecuteSubscribers<IEntityUpdatedDomainEventSubscriber<TEntity>, EntityUpdatedDomainEvent<TEntity>>(
                domainEvent,
                (subscriber, de, ct) => subscriber.EntityUpdatedAsync(de, ct),
                cancellationToken);
        }
    }

    private async Task ExecuteSubscribers<TSubscriber, TEvent>(
        TEvent domainEvent,
        Func<TSubscriber, TEvent, CancellationToken, Task> func,
        CancellationToken cancellationToken)
        where TSubscriber : IDisposable
    {
        using var scope = _serviceScopeFactory.CreateScope();

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
                    await func(x, domainEvent, cancellationToken);
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            });

        await Task.WhenAll(tasks);

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
