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
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly IDomainEventContextProvider _domainEventContextProvider;

        public EntityFrameworkDeleteClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(contextProvider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            var entity = new TEntity
            {
                Id = key
            };

            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet(context);

                var found = await GetByKeyAsync(context, key, cancellationToken);

                if (found != null)
                {
                    entity = found;
                    set.Remove(found);
                }
                else
                {
                    set.Attach(entity);
                    entry.State = EntityState.Deleted;
                    entity = entry.Entity;
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
            if (!(_domainEventPublisher is DefaultEntityDomainEventPublisher))
            {
                var domainEvent = new EntityDeletedDomainEvent<TEntity>
                {
                    Entity = savedEntity,
                    EventContext = _domainEventContextProvider.GetContext()
                };

                return _domainEventPublisher.PublishEntityDeleteEventAsync(domainEvent, cancellationToken);
            }
            
            return Task.CompletedTask;
        }
    }
}