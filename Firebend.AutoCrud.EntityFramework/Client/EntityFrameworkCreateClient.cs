using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class EntityFrameworkCreateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkCreateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IDomainEventPublisherService<TKey, TEntity> _publisherService;
    private readonly IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> _exceptionHandler;

    public EntityFrameworkCreateClient(IDbContextProvider<TKey, TEntity> provider,
        IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(provider)
    {
        _exceptionHandler = exceptionHandler;
        _publisherService = publisherService;
    }

    protected virtual Task<TEntity> OnBeforeAddAsync(IDbContext context, TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
        => Task.FromResult(entity);

    protected virtual async Task<TEntity> AddInternalAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        TEntity savedEntity;

        await using (var context = await GetDbContextAsync(transaction, cancellationToken))
        {
            savedEntity = await AddEntityToDbContextAsync(entity, transaction, context, cancellationToken);

            await SaveAddChangesAsync(entity, context, cancellationToken);
        }

        if (_publisherService is null)
        {
            return savedEntity;
        }

        return await _publisherService.ReadAndPublishAddedEventAsync(savedEntity.Id, null, cancellationToken);
    }

    private async Task<TEntity> AddEntityToDbContextAsync(TEntity entity,
        IEntityTransaction transaction,
        IDbContext context,
        CancellationToken cancellationToken)
    {
        var set = GetDbSet(context);

        if (entity is IModifiedEntity modified)
        {
            var now = DateTimeOffset.Now;

            modified.CreatedDate = now;
            modified.ModifiedDate = now;
        }

        entity = await OnBeforeAddAsync(context, entity, transaction, cancellationToken);

        var entry = await set.AddAsync(entity, cancellationToken);

        return entry.Entity;
    }

    private async Task SaveAddChangesAsync(TEntity entity, IDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!(_exceptionHandler?.HandleException(context, entity, ex) ?? false))
            {
                throw;
            }
        }
    }

    public virtual Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        => AddInternalAsync(entity, null, cancellationToken);

    public virtual Task<TEntity> AddAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
        => AddInternalAsync(entity, transaction, cancellationToken);
}
