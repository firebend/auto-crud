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
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    internal static class EntityFrameworkUpdateClientCaches<TEntity>
    {
        static EntityFrameworkUpdateClientCaches()
        {
            var props = typeof(TEntity)
                .GetProperties()
                .Where(x => x.GetCustomAttribute<AutoCrudIgnoreUpdate>() != null)
                .Select(x => x.Name)
                .ToList();

            props.Add(nameof(IModifiedEntity.CreatedDate));

            IgnoredProperties = props.ToArray();
        }

        public static readonly string[] IgnoredProperties;
    }

    public abstract class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
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
        }

        protected virtual bool IsDefaultEventPublisher() => _domainEventPublisher is null or DefaultEntityDomainEventPublisher;

        /// <summary>
        /// Occurs when an entity is being PUT into the database and did not previously exist. Occurs before the entity is added.
        /// </summary>
        /// <param name="context">
        /// The db context
        /// </param>
        /// <param name="entity">
        /// The entity being updated
        /// </param>
        /// <param name="transaction">
        /// The transaction if any being used during the operation
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token
        /// </param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <returns></returns>
        protected virtual Task<TEntity> OnBeforeReplaceAsync(IDbContext context, TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
            => Task.FromResult(entity);

        /// <summary>
        /// Occurs when an entity is being PUT into the database and  previously exist. Occurs before the entity is updated.
        /// </summary>
        /// <param name="context">
        /// The db context
        /// </param>
        /// <param name="entity">
        /// The entity being updated
        /// </param>
        /// <param name="transaction">
        /// The transaction if any being used during the operation
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token
        /// </param>
        /// <returns>
        /// The entity
        /// </returns>
        /// <returns></returns>
        protected virtual Task<TEntity> OnBeforeUpdateAsync(IDbContext context, TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
            => Task.FromResult(entity);


        /// <summary>
        /// Occurs during a PATCH update after the patch is applied but before saving.
        /// </summary>
        /// <param name="context">
        /// The db context
        /// </param>
        /// <param name="entity">
        /// The entity being updated
        /// </param>
        /// <param name="transaction">
        /// The transaction if any being used during the operation
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token
        /// </param>
        /// <returns>
        /// The entity
        /// </returns>
        protected virtual Task<TEntity> OnBeforePatchAsync(IDbContext context, TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
            => Task.FromResult(entity);

        protected virtual async Task<TEntity> UpdateInternalAsync(TKey key,
            JsonPatchDocument<TEntity> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            await using var context = await GetDbContextAsync(entityTransaction, cancellationToken).ConfigureAwait(false);
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

            entity = await OnBeforePatchAsync(context, entity, entityTransaction, cancellationToken);

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

            await PublishDomainEventAsync(original, jsonPatchDocument, entityTransaction, cancellationToken);

            return entity;
        }

        protected virtual async Task<TEntity> UpdateInternalAsync(TEntity entity,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            await using var context = await GetDbContextAsync(transaction, cancellationToken)
                .ConfigureAwait(false);

            var model = await GetByEntityKeyAsync(context, entity.Id, false, cancellationToken)
                .ConfigureAwait(false);

            var original = model?.Clone();

            if (original != null)
            {
                model = entity.CopyPropertiesTo(model, EntityFrameworkUpdateClientCaches<TEntity>.IgnoredProperties);

                if (model is IModifiedEntity modified)
                {
                    modified.ModifiedDate = DateTimeOffset.Now;
                }

                model = await OnBeforeUpdateAsync(context, model, transaction, cancellationToken);
            }
            else
            {
                if (entity is IModifiedEntity modified)
                {
                    var now = DateTimeOffset.Now;
                    modified.CreatedDate = now;
                    modified.ModifiedDate = now;
                }

                var set = GetDbSet(context);

                entity = await OnBeforeReplaceAsync(context, entity, transaction, cancellationToken);

                var entry = await set
                    .AddAsync(entity, cancellationToken)
                    .ConfigureAwait(false);

                model = entry.Entity;
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

            if (original == null)
            {
                await PublishAddedDomainEventAsync(model, transaction, cancellationToken);
                return model;
            }

            JsonPatchDocument<TEntity> jsonPatchDocument = null;

            if (!IsDefaultEventPublisher())
            {
                jsonPatchDocument = _jsonPatchDocumentGenerator.Generate(original, model);
            }

            await PublishDomainEventAsync(original, jsonPatchDocument, transaction, cancellationToken);

            return model;
        }

        public virtual Task<TEntity> UpdateAsync(TEntity entity,
            CancellationToken cancellationToken = default)
            => UpdateInternalAsync(entity, null, cancellationToken);

        public virtual Task<TEntity> UpdateAsync(TEntity entity,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => UpdateInternalAsync(entity, entityTransaction, cancellationToken);

        public virtual Task<TEntity> UpdateAsync(TKey key,
            JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken = default)
            => UpdateAsync(key, patch, null, cancellationToken);

        public virtual Task<TEntity> UpdateAsync(TKey key,
            JsonPatchDocument<TEntity> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => UpdateInternalAsync(key, jsonPatchDocument, entityTransaction, cancellationToken);

        protected virtual Task PublishDomainEventAsync(TEntity previous,
            JsonPatchDocument<TEntity> patch,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            if (IsDefaultEventPublisher())
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityUpdatedDomainEvent<TEntity>
            {
                Previous = previous,
                OperationsJson = JsonConvert.SerializeObject(patch?.Operations, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                EventContext = _domainEventContextProvider?.GetContext()
            };

            return _domainEventPublisher.PublishEntityUpdatedEventAsync(domainEvent, transaction, cancellationToken);

        }

        protected virtual Task PublishAddedDomainEventAsync(TEntity entity,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            if (IsDefaultEventPublisher())
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityAddedDomainEvent<TEntity> { Entity = entity, EventContext = _domainEventContextProvider?.GetContext() };
            return _domainEventPublisher.PublishEntityAddEventAsync(domainEvent, entityTransaction, cancellationToken);
        }
    }
}
