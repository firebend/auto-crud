using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Attributes;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly bool _isDefaultPublisher;
        private readonly IJsonPatchGenerator _jsonPatchDocumentGenerator;
        private readonly IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> _exceptionHandler;

        protected EntityFrameworkUpdateClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchGenerator jsonPatchDocumentGenerator,
            IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler) : base(contextProvider)
        {
            _domainEventPublisher = domainEventPublisher;
            _domainEventContextProvider = domainEventContextProvider;
            _jsonPatchDocumentGenerator = jsonPatchDocumentGenerator;
            _exceptionHandler = exceptionHandler;
            _isDefaultPublisher = domainEventPublisher is DefaultEntityDomainEventPublisher;
        }

        private readonly Lazy<string[]> _ignoredProperties = new(() =>
        {
            var props = typeof(TEntity)
                .GetProperties()
                .Where(x => x.GetCustomAttribute<AutoCrudIgnoreUpdate>() != null)
                .Select(x => x.Name)
                .ToList();

            props.Add(nameof(IModifiedEntity.CreatedDate));

            return props.ToArray();
        });

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var context = await GetDbContextAsync(cancellationToken).ConfigureAwait(false);

            var model = await GetByEntityKeyAsync(context, entity.Id, false, cancellationToken).ConfigureAwait(false);

            if (model == null)
            {
                return null;
            }

            var original = model.Clone();

            model = entity.CopyPropertiesTo(model, _ignoredProperties.Value);

            if (model is IModifiedEntity modified)
            {
                modified.ModifiedDate = DateTimeOffset.Now;
            }

            try
            {
                await context
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                if (!(_exceptionHandler?.HandleException(context, entity, ex) ?? false))
                {
                    throw;
                }
            }

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
            var entity = await GetByEntityKeyAsync(context, key, false, cancellationToken).ConfigureAwait(false);

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

            try
            {
                await context
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex)
            {
                if (!(_exceptionHandler?.HandleException(context, entity, ex) ?? false))
                {
                    throw;
                }
            }

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
                    OperationsJson = JsonConvert.SerializeObject(patch?.Operations, Formatting.None, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }),
                    EventContext = _domainEventContextProvider?.GetContext()
                };

                return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
