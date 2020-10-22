using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Extensions.Primitives;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkCreateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly IDomainEventContextProvider _domainEventContextProvider;

        public EntityFrameworkCreateClient(IDbContextProvider<TKey, TEntity> provider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider) : base(provider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var set = GetDbSet(context);
                
            var entry = await set
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            var savedEntity = entry.Entity;
            
            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await PublishDomainEventAsync(savedEntity, cancellationToken).ConfigureAwait(false);

            return savedEntity;
        }

        private Task PublishDomainEventAsync(TEntity savedEntity, CancellationToken cancellationToken = default)
        {
            if (!(_domainEventPublisher is DefaultEntityDomainEventPublisher))
            {
                var domainEvent = new EntityAddedDomainEvent<TEntity>
                {
                    Entity = savedEntity,
                    EventContext = _domainEventContextProvider.GetContext()
                };

                return _domainEventPublisher.PublishEntityAddEventAsync(domainEvent, cancellationToken);
            }
            
            return Task.CompletedTask;
        }
    }
}