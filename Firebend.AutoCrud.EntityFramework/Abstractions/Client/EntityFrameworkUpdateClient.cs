using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly bool _isDefaultPublisher;
        private readonly IJsonPatchDocumentGenerator _jsonPatchDocumentGenerator;

        public EntityFrameworkUpdateClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchDocumentGenerator jsonPatchDocumentGenerator) : base(contextProvider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
            _jsonPatchDocumentGenerator = jsonPatchDocumentGenerator;
            _isDefaultPublisher = domainEventPublisher is DefaultEntityDomainEventPublisher;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);

            var model = await GetByKeyAsync(context, entity.Id, cancellationToken).ConfigureAwait(false);

            if (model == null)
            {
                return null;
            }

            var original = model.Clone();

            model = entity.CopyPropertiesTo(model, nameof(IModifiedEntity.CreatedDate));

            if (model is IModifiedEntity modified)
            {
                modified.ModifiedDate = DateTimeOffset.Now;
            }

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            JsonPatchDocument<TEntity> jsonPatchDocument = null;

            if (!_isDefaultPublisher)
            {
                jsonPatchDocument = _jsonPatchDocumentGenerator.Generate(original, model);
            }

            await PublishDomainEventAsync(original, jsonPatchDocument, cancellationToken);

            return model;
        }

        public virtual async Task<TEntity> UpdateAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);
            var entity = await GetByKeyAsync(context, key, cancellationToken).ConfigureAwait(false);

            if (entity == null)
            {
                return null;
            }

            var original = entity.Clone();

            if (entity is IModifiedEntity)
            {
                jsonPatchDocument.Operations.Add(new Operation<TEntity>(
                    "replace",
                    $"/{nameof(IModifiedEntity.ModifiedDate)}",
                    null,
                    DateTimeOffset.Now));
            }

            jsonPatchDocument.ApplyTo(entity);

            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            await PublishDomainEventAsync(original, jsonPatchDocument, cancellationToken);

            return entity;
        }

        private Task PublishDomainEventAsync(TEntity previous, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default)
        {
            if (_domainEventPublisher != null && !_isDefaultPublisher)
            {
                var domainEvent = new EntityUpdatedDomainEvent<TEntity>
                {
                    Previous = previous,
                    Patch = patch,
                    EventContext = _domainEventContextProvider?.GetContext()
                };

                return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
