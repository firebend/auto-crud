using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextSaveRepo<TKey, TEntity>, IEntityFrameworkUpdateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IDomainEventPublisherService<TKey, TEntity> _publisherService;
    private readonly IEntityReadService<TKey, TEntity> _readService;

    public EntityFrameworkUpdateClient(IDbContextProvider<TKey, TEntity> contextProvider,
        IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler,
        IEntityReadService<TKey, TEntity> readService,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(contextProvider, exceptionHandler)
    {
        _publisherService = publisherService;
        _readService = readService;
    }

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
        var previous = await _readService.GetByKeyAsync(key, cancellationToken);

        if (previous is null)
        {
            return null;
        }

        await using var context = await GetDbContextAsync(entityTransaction, cancellationToken);
        var entity = await GetByEntityKeyAsync(context, key, false, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        if (entity is IModifiedEntity)
        {
            jsonPatchDocument.Operations.Add(new Operation<TEntity>(
                "replace",
                $"/{nameof(IModifiedEntity.ModifiedDate)}",
                null,
                DateTimeOffset.UtcNow));
        }

        jsonPatchDocument.ApplyTo(entity);

        entity = await OnBeforePatchAsync(context, entity, entityTransaction, cancellationToken);

        await SaveAsync(entity, context, cancellationToken);

        var fetched = await _publisherService.ReadAndPublishUpdateEventAsync(entity.Id, previous, entityTransaction, jsonPatchDocument, cancellationToken);

        return fetched;
    }

    protected virtual async Task<TEntity> UpdateInternalAsync(TEntity entity,
        IEntityTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        var previous = await _readService.GetByKeyAsync(entity.Id, transaction, cancellationToken);
        var isUpdating = previous is not null;
        TEntity savedEntity;

        await using (var context = await GetDbContextAsync(transaction, cancellationToken))
        {
            savedEntity = isUpdating
                ? await UpdateEntityAsync(entity, transaction, context, cancellationToken)
                : await AddEntityAsync(entity, transaction, context, cancellationToken);
        }


        if (_publisherService is null)
        {
            return savedEntity;
        }

        if (isUpdating is false)
        {
            return await _publisherService.ReadAndPublishAddedEventAsync(savedEntity.Id, transaction, cancellationToken);
        }

        return await _publisherService.ReadAndPublishUpdateEventAsync(savedEntity.Id, previous, transaction, cancellationToken);
    }

    private async Task<TEntity> AddEntityAsync(TEntity entity, IEntityTransaction transaction, IDbContext context, CancellationToken cancellationToken)
    {
        if (entity is IModifiedEntity modified)
        {
            var now = DateTimeOffset.UtcNow;
            modified.CreatedDate = now;
            modified.ModifiedDate = now;
        }

        var set = GetDbSet(context);

        entity = await OnBeforeReplaceAsync(context, entity, transaction, cancellationToken);

        await set.AddAsync(entity, cancellationToken);

        await SaveAsync(entity, context, cancellationToken);

        return entity;
    }

    private async Task<TEntity> UpdateEntityAsync(TEntity entity, IEntityTransaction transaction, IDbContext context, CancellationToken cancellationToken)
    {
        var model = await GetByEntityKeyAsync(context, entity.Id, false, cancellationToken);

        model = entity.CopyPropertiesTo(model, [nameof(IModifiedEntity.CreatedDate)]);

        if (model is IModifiedEntity modified)
        {
            modified.ModifiedDate = DateTimeOffset.UtcNow;
        }

        model = await OnBeforeUpdateAsync(context, model, transaction, cancellationToken);

        await SaveAsync(model, context, cancellationToken);

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

}
