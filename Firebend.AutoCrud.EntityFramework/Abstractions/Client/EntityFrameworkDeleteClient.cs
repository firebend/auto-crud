using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
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

        public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(cancellationToken)
                .ConfigureAwait(false);

            var entity = new TEntity { Id = key };
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet(context);

                var found = await GetByEntityKeyAsync(context, key, false, cancellationToken)
                    .ConfigureAwait(false);

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

        public virtual async Task<IEnumerable<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var query = await GetFilteredQueryableAsync(context, false, cancellationToken);
            var set = context.Set<TEntity>();
            var list = await query
                .Where(filter)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (list.IsEmpty())
            {
                return null;
            }

            set.RemoveRange(list);

            await context.SaveChangesAsync(cancellationToken);

            var tasks = list.Select(entity => PublishDomainEventAsync(entity, cancellationToken)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return list;
        }

        public Task<TEntity> DeleteAsync(TKey filter,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken)
            => throw new NotImplementedException(); //todo

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
