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
    public abstract class EntityFrameworkDeleteClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;

        protected EntityFrameworkDeleteClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(contextProvider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public virtual async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);

            var entity = new TEntity { Id = key };

            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet(context);

                var found = await GetByKeyAsync(context, key, false, cancellationToken);

                if (found != null)
                {
                    entity = found;
                    set.Remove(found);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                entity = entry.Entity;
                entry.State = EntityState.Deleted;
            }

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await PublishDomainEventAsync(entity, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        private Task PublishDomainEventAsync(TEntity savedEntity, CancellationToken cancellationToken = default)
        {
            if (_domainEventPublisher == null || _domainEventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityDeletedDomainEvent<TEntity> { Entity = savedEntity, EventContext = _domainEventContextProvider?.GetContext() };

            return _domainEventPublisher.PublishEntityDeleteEventAsync(domainEvent, cancellationToken);

        }
    }
}
