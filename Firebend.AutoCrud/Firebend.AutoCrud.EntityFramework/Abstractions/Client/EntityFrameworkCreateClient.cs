#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;

#endregion

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
            var entry = await GetDbSet()
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);

            var savedEntity = entry.Entity;

            await Context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await _domainEventPublisher
                .PublishEntityAddEventAsync(savedEntity, cancellationToken)
                .ConfigureAwait(false);

            return savedEntity;
        }
    }
}