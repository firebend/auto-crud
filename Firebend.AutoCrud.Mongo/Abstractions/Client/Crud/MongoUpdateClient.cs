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
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoUpdateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IMongoCollectionKeyGenerator<TKey, TEntity> _keyGenerator;
        private readonly IEntityDomainEventPublisher _domainEventPublisher;
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IJsonPatchDocumentGenerator _jsonPatchDocumentGenerator;
        private readonly bool _isDefaultPublisher;

        public MongoUpdateClient(IMongoClient client,
            ILogger<MongoUpdateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchDocumentGenerator jsonPatchDocumentGenerator,
            IEntityDomainEventPublisher domainEventPublisher) : base(client, logger, entityConfiguration)
        {
            _keyGenerator = keyGenerator;
            _domainEventContextProvider = domainEventContextProvider;
            _jsonPatchDocumentGenerator = jsonPatchDocumentGenerator;
            _domainEventPublisher = domainEventPublisher;
            _isDefaultPublisher = domainEventPublisher is DefaultEntityDomainEventPublisher;
        }

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), false, cancellationToken);
        }

        public Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), true, cancellationToken);
        }

        public Task<TEntity> UpsertAsync(TEntity entity, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return UpdateInternalAsync(entity, filter, true, cancellationToken);
        }

        public async Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection();

            var ids = await UpdateManyInternalAsync(collection, entities, cancellationToken)
                .ConfigureAwait(false);

            if (ids.Any())
            {
                var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

                var updatedEntities = await RetryErrorAsync(() => collection
                        .AsQueryable()
                        .Where(_ => filter.Inject())
                        .ToListAsync(cancellationToken))
                    .ConfigureAwait(false);

                return updatedEntities;
            }

            return new List<TEntity>();
        }

        public async Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities, Expression<Func<TEntity, TOut>> projection,
            CancellationToken cancellationToken = default)
        {
            var collection = GetCollection();

            var ids = await UpdateManyInternalAsync(collection, entities, cancellationToken)
                .ConfigureAwait(false);

            if (ids.Any())
            {
                var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

                var updatedEntities = await RetryErrorAsync(() => collection
                        .AsQueryable()
                        .Where(_ => filter.Inject())
                        .Select(projection)
                        .ToListAsync(cancellationToken))
                    .ConfigureAwait(false);

                return updatedEntities;
            }

            return new List<TOut>();
        }

        public virtual async Task<TEntity> UpdateAsync(TKey id, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default)
        {
            var mongoCollection = GetCollection();

            var filter = await BuildFiltersAsync(x => x.Id.Equals(id), cancellationToken);

            var entity = await mongoCollection
                .AsQueryable()
                .Where(filter)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
                return null;

            patch.ApplyTo(entity);

            return await UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), false, cancellationToken);
        }

        protected virtual async Task<TEntity> UpdateInternalAsync(TEntity entity,
            Expression<Func<TEntity, bool>> filter,
            bool doUpsert,
            CancellationToken cancellationToken,
            JsonPatchDocument<TEntity> patchDocument = null)
        {
            var filters = await BuildFiltersAsync(filter, cancellationToken);
            var filtersDefinition = Builders<TEntity>.Filter.Where(filters);
            var mongoCollection = GetCollection();

            var now = DateTimeOffset.Now;
            if (entity is IModifiedEntity modifiedEntity)
            {
                modifiedEntity.ModifiedDate = now;
            }

            var original = await RetryErrorAsync(() =>
                mongoCollection.Find(filtersDefinition).SingleOrDefaultAsync(cancellationToken));

            var modified = original == null ? new TEntity() : original.Clone();

            entity.CopyPropertiesTo(modified, nameof(IModifiedEntity.CreatedDate));

            if (original == null && modified is IModifiedEntity mod)
            {
                mod.CreatedDate = now;
            }

            var result = await RetryErrorAsync(() =>
                mongoCollection.FindOneAndReplaceAsync(
                    filtersDefinition,
                    modified,
                    new FindOneAndReplaceOptions<TEntity, TEntity>
                    {
                        ReturnDocument = ReturnDocument.After,
                        IsUpsert = doUpsert
                    },
                    cancellationToken));

            if (original != null)
            {
                patchDocument ??= _jsonPatchDocumentGenerator.Generate(original, modified);

                await PublishUpdatedDomainEventAsync(original, patchDocument, cancellationToken).ConfigureAwait(false);

                return result;
            }

            if (doUpsert)
            {
                await PublishAddedDomainEventAsync(result, cancellationToken).ConfigureAwait(false);

                return result;
            }

            return null;
        }

        protected virtual async Task<List<TKey>> UpdateManyInternalAsync(IMongoCollection<TEntity> mongoCollection,
            List<EntityUpdate<TEntity>> entities,
            CancellationToken cancellationToken = default)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("There are no entities provided to update.", nameof(entities));

            var ids = new List<TKey>();

            foreach (var entityUpdate in entities)
            {
                var id = await mongoCollection
                    .AsQueryable()
                    .Where(entityUpdate.Filter)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

                var isCreating = false;
                if (id.Equals(default))
                {
                    id = await _keyGenerator.GenerateKeyAsync(cancellationToken).ConfigureAwait(false);
                    isCreating = true;
                }

                entityUpdate.Entity.Id = id;
                if (entityUpdate.Entity is IModifiedEntity modified)
                {
                    var now = DateTimeOffset.Now;
                    if (isCreating)
                    {
                        modified.CreatedDate = now;
                    }

                    modified.ModifiedDate = now;
                }
                ids.Add(id);
            }

            var writes = entities
                .Select(x => new ReplaceOneModel<TEntity>(Builders<TEntity>
                    .Filter
                    .Where(y => y.Id.Equals(x.Entity.Id)), x.Entity)
                {
                    IsUpsert = true
                });

            await RetryErrorAsync(() => mongoCollection.BulkWriteAsync(
                writes,
                new BulkWriteOptions
                {
                    IsOrdered = true
                }, cancellationToken));

            return ids;
        }

        private Task PublishUpdatedDomainEventAsync(TEntity previous, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default)
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

        private Task PublishAddedDomainEventAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (_domainEventPublisher != null && !_isDefaultPublisher)
            {
                var domainEvent = new EntityAddedDomainEvent<TEntity>
                {
                    Entity = entity,
                    EventContext = _domainEventContextProvider?.GetContext()
                };

                return _domainEventPublisher.PublishEntityAddEventAsync(domainEvent, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
