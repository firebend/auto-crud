#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityDomainEventPublisher _domainEventPublisher;

        public EntityFrameworkUpdateClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher) : base(contextProvider)
        {
            _domainEventPublisher = domainEventPublisher;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return await UpdateInternalAsync(entity, null, cancellationToken);
        }

        public async Task<TEntity> UpdateAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            var original = await GetByKeyAsync(key, cancellationToken).ConfigureAwait(false);

            if (original == null) return null;

            var entity = original.Clone();

            jsonPatchDocument.ApplyTo(entity);

            return await UpdateInternalAsync(entity, original, cancellationToken).ConfigureAwait(false);
        }

        private async Task<TEntity> UpdateInternalAsync(TEntity entity, TEntity original, CancellationToken cancellationToken)
        {
            var set = GetDbSet();

            if (original == null)
            {
                original = await GetByKeyAsync(entity.Id, cancellationToken).ConfigureAwait(false);

                if (original == null)
                {
                    return null;
                }

                entity.CopyPropertiesTo(original, "Id");
            }

            var entry = set.Update(entity);

            await Context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            var modified = entry.Entity;

            await _domainEventPublisher
                .PublishEntityUpdatedEventAsync(original, modified, cancellationToken)
                .ConfigureAwait(false);

            return modified;
        }
    }
}