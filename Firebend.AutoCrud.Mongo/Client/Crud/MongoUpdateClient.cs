using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Firebend.AutoCrud.Mongo.Client.Crud;

public class MongoUpdateClient<TKey, TEntity> : MongoClientBaseEntity<TKey, TEntity>, IMongoUpdateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IDomainEventPublisherService<TKey, TEntity> _domainEventPublisher;
    private readonly IMongoCollectionKeyGenerator<TKey, TEntity> _keyGenerator;

    public MongoUpdateClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoUpdateClient<TKey, TEntity>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator,
        IMongoRetryService retryService,
        IMongoReadPreferenceService readPreferenceService,
        IDomainEventPublisherService<TKey, TEntity> domainEventPublisher = null) : base(
            clientFactory,
            logger,
            entityConfiguration,
            retryService,
            readPreferenceService)
    {
        _keyGenerator = keyGenerator;
        _domainEventPublisher = domainEventPublisher;
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken) =>
        UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), false, null, null, null, cancellationToken);

    public virtual Task<TEntity> UpsertAsync(TEntity entity, CancellationToken cancellationToken) =>
        UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), true, null, null, null, cancellationToken);

    public virtual Task<TEntity> UpsertAsync(TEntity entity,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
        => UpdateInternalAsync(entity, x => x.Id.Equals(entity.Id), true, transaction, null, null, cancellationToken);

    public virtual Task<TEntity> UpsertAsync(TEntity entity,
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
        => UpdateInternalAsync(entity, filter, true, null, null, null, cancellationToken);

    public virtual Task<TEntity> UpsertAsync(TEntity entity,
        Expression<Func<TEntity, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
        => UpdateInternalAsync(entity, filter, true, transaction, null, null, cancellationToken);

    public virtual Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities,
        CancellationToken cancellationToken)
        => UpsertManyAsync(entities, null, cancellationToken);

    public virtual async Task<List<TEntity>> UpsertManyAsync(List<EntityUpdate<TEntity>> entities,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync(null, cancellationToken);

        var ids = await UpdateManyInternalAsync(collection, null, entities, cancellationToken);

        if (ids.Count != 0)
        {
            var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

            var updatedEntities = await RetryErrorAsync(() => collection
                    .AsQueryable(EntityConfiguration.AggregateOption)
                    .Where(_ => filter.Inject())
                    .ToListAsync(cancellationToken));

            return updatedEntities;
        }

        return [];
    }

    public virtual Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities,
        Expression<Func<TEntity, TOut>> projection,
        CancellationToken cancellationToken)
        => UpsertManyAsync(entities, projection, null, cancellationToken);

    public virtual async Task<List<TOut>> UpsertManyAsync<TOut>(List<EntityUpdate<TEntity>> entities,
        Expression<Func<TEntity, TOut>> projection,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var collection = await GetCollectionAsync(null, cancellationToken);

        var ids = await UpdateManyInternalAsync(collection, transaction, entities, cancellationToken);

        if (ids.Count != 0)
        {
            var filter = Builders<TEntity>.Filter.In(x => x.Id, ids);

            var updatedEntities = await RetryErrorAsync(() => collection
                    .AsQueryable(EntityConfiguration.AggregateOption)
                    .Where(_ => filter.Inject())
                    .Select(projection)
                    .ToListAsync(cancellationToken));

            return updatedEntities;
        }

        return [];
    }

    public virtual Task<TEntity> UpdateAsync(TKey id,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken) =>
        UpdateAsync(id, patch, null, cancellationToken);

    public virtual async Task<TEntity> UpdateAsync(TKey id,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var queryable = await GetFilteredCollectionAsync(
            entities => Task.FromResult(entities.Where(x => x.Id.Equals(id))),
                    transaction,
                    cancellationToken);

        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var original = entity.Clone();

        patch.ApplyTo(entity);

        return await UpdateInternalAsync(entity,
            x => x.Id.Equals(entity.Id),
            false,
            transaction,
            patch,
            original,
            cancellationToken);
    }

    protected virtual async Task<TEntity> UpdateInternalAsync(TEntity entity,
        Expression<Func<TEntity, bool>> filter,
        bool doUpsert,
        IEntityTransaction transaction,
        JsonPatchDocument<TEntity> patchDocument,
        TEntity original,
        CancellationToken cancellationToken)
    {
        var filters = await BuildFiltersAsync(filter, cancellationToken);
        var filtersDefinition = Builders<TEntity>.Filter.Where(filters);
        var mongoCollection = await GetCollectionAsync(null, cancellationToken);

        var now = DateTimeOffset.Now;

        if (entity is IModifiedEntity modifiedEntity)
        {
            modifiedEntity.ModifiedDate = now;
        }

        if (original == null)
        {
            var query = await GetFilteredCollectionAsync(
                entities => Task.FromResult(entities.Where(x => x.Id.Equals(entity.Id))),
                transaction,
                cancellationToken);

            original = await query.FirstOrDefaultAsync(cancellationToken);
        }

        var modified = original == null ? new TEntity() : original.Clone();

        entity.CopyPropertiesTo(modified, [nameof(IModifiedEntity.CreatedDate)]);

        if (original == null && modified is IModifiedEntity mod)
        {
            mod.CreatedDate = now;
        }

        TEntity result;
        var session = UnwrapSession(transaction);

        if (session != null)
        {
            result = await RetryErrorAsync(() =>
                mongoCollection.FindOneAndReplaceAsync(
                    session,
                    filtersDefinition,
                    modified,
                    new FindOneAndReplaceOptions<TEntity, TEntity>
                    {
                        ReturnDocument = ReturnDocument.After,
                        IsUpsert = doUpsert
                    },
                    cancellationToken));
        }
        else
        {
            result = await RetryErrorAsync(() =>
                mongoCollection.FindOneAndReplaceAsync(
                    filtersDefinition,
                    modified,
                    new FindOneAndReplaceOptions<TEntity, TEntity>
                    {
                        ReturnDocument = ReturnDocument.After,
                        IsUpsert = doUpsert
                    },
                    cancellationToken));
        }

        if (original is not null && _domainEventPublisher is not null)
        {
            return patchDocument is null
                ? await _domainEventPublisher.ReadAndPublishUpdateEventAsync(original.Id, original, transaction, cancellationToken)
                : await _domainEventPublisher.ReadAndPublishUpdateEventAsync(original.Id, original, transaction, patchDocument, cancellationToken);
        }

        if (doUpsert && _domainEventPublisher is not null)
        {
            return await _domainEventPublisher.ReadAndPublishAddedEventAsync(result.Id, transaction, cancellationToken);
        }

        return result;
    }

    protected virtual async Task<List<TKey>> UpdateManyInternalAsync(IMongoCollection<TEntity> mongoCollection,
        IEntityTransaction entityTransaction,
        List<EntityUpdate<TEntity>> entities,
        CancellationToken cancellationToken)
    {
        if (entities == null || entities.Count == 0)
        {
            throw new ArgumentException("There are no entities provided to update.", nameof(entities));
        }

        var ids = new List<TKey>();

        IClientSessionHandle session = null;

        if (entityTransaction != null)
        {
            session = UnwrapSession(entityTransaction);
        }

        var queryable = session == null
            ? mongoCollection.AsQueryable(EntityConfiguration.AggregateOption)
            : mongoCollection.AsQueryable(session, EntityConfiguration.AggregateOption);

        foreach (var entityUpdate in entities)
        {
            var id = await queryable
                .Where(entityUpdate.Filter)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var isCreating = false;

            if (id.Equals(default(TKey)))
            {
                id = await _keyGenerator.GenerateKeyAsync(cancellationToken);
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
            { IsUpsert = true });

        if (session != null)
        {
            await RetryErrorAsync(() => mongoCollection.BulkWriteAsync(
                    session,
                    writes,
                    new BulkWriteOptions { IsOrdered = true }, cancellationToken));
        }
        else
        {
            await RetryErrorAsync(() => mongoCollection.BulkWriteAsync(
                    writes,
                    new BulkWriteOptions { IsOrdered = true }, cancellationToken));
        }

        return ids;
    }
}
