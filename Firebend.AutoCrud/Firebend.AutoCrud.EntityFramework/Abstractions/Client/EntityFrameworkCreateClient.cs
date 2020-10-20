using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkCreateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityDomainEventPublisher _domainEventPublisher;

        public EntityFrameworkCreateClient(IDbContextProvider<TKey, TEntity> provider,
            IEntityDomainEventPublisher domainEventPublisher) : base(provider)
        {
            _domainEventPublisher = domainEventPublisher;
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

            await _domainEventPublisher
                .PublishEntityAddEventAsync(savedEntity, cancellationToken)
                .ConfigureAwait(false);

            return savedEntity;
        }
    }
}