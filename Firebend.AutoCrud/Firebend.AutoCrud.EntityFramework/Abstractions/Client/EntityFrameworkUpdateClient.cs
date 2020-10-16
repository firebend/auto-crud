using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

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
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            var model = await GetByKeyAsync(context, entity.Id, cancellationToken).ConfigureAwait(false);

            if (model == null)
            {
                return null;
            }

            var original = model.Clone();
            model = entity.CopyPropertiesTo(model);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await _domainEventPublisher
                .PublishEntityUpdatedEventAsync(original, model, cancellationToken)
                .ConfigureAwait(false);

            return model;
        }

        public async Task<TEntity> UpdateAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var entity = await GetByKeyAsync(context, key, cancellationToken).ConfigureAwait(false);

            if (entity == null)
            {
                return null;
            }

            var original = entity.Clone();

            jsonPatchDocument.ApplyTo(original);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await _domainEventPublisher
                .PublishEntityUpdatedEventAsync(original, entity, cancellationToken);

            return entity;
        }
    }
}