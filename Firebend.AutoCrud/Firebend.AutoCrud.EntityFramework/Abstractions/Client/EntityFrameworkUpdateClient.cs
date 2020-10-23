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

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IJsonPatchDocumentGenerator _jsonPatchDocumentGenerator;
        private readonly bool _isDefaultPublisher;

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

            JsonPatchDocument<TEntity> jsonPatchDocument = null;

            if (!_isDefaultPublisher)
            {
                jsonPatchDocument = _jsonPatchDocumentGenerator.Generate(original, model);
            }
            
            await PublishDomainEventAsync(entity, jsonPatchDocument, cancellationToken);

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

            await PublishDomainEventAsync(entity, jsonPatchDocument, cancellationToken);

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