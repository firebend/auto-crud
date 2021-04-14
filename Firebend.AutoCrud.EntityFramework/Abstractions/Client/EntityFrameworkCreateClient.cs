using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkCreateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> _exceptionHandler;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;

        protected EntityFrameworkCreateClient(IDbContextProvider<TKey, TEntity> provider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider,
            IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler) : base(provider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
            _exceptionHandler = exceptionHandler;
        }

        protected virtual async Task<TEntity> AddInternalAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(transaction, cancellationToken).ConfigureAwait(false);

            var set = GetDbSet(context);

            if (entity is IModifiedEntity modified)
            {
                var now = DateTimeOffset.Now;

                modified.CreatedDate = now;
                modified.ModifiedDate = now;
            }

            var entry = await set
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            var savedEntity = entry.Entity;

            try
            {
                await context
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                if (!(_exceptionHandler?.HandleException(context, entity, ex) ?? false))
                {
                    throw;
                }
            }

            await PublishDomainEventAsync(savedEntity, transaction, cancellationToken).ConfigureAwait(false);

            return savedEntity;
        }

        public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
            => AddInternalAsync(entity, null, cancellationToken);

        public virtual Task<TEntity> AddAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
            => AddInternalAsync(entity, transaction, cancellationToken);

        private Task PublishDomainEventAsync(TEntity savedEntity, IEntityTransaction transaction,  CancellationToken cancellationToken = default)
        {
            if (_domainEventPublisher == null || _domainEventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = savedEntity, EventContext = _domainEventContextProvider?.GetContext() };

            return _domainEventPublisher.PublishEntityAddEventAsync(domainEvent, transaction, cancellationToken);

        }
    }
}
