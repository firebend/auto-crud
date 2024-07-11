using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class EntityFrameworkDeleteClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkDeleteClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    private readonly IDomainEventPublisherService<TKey, TEntity> _publisherService;
    private readonly IEntityReadService<TKey, TEntity> _readService;

    public EntityFrameworkDeleteClient(IDbContextProvider<TKey, TEntity> contextProvider,
        IEntityReadService<TKey, TEntity> readService,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(contextProvider)
    {
        _publisherService = publisherService;
        _readService = readService;
    }

    protected virtual async Task<TEntity> DeleteInternalAsync(TKey key, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        var previous = await _readService.GetByKeyAsync(key, cancellationToken);

        if (previous is null)
        {
            return null;
        }

        await using var context = await GetDbContextAsync(transaction, cancellationToken);

        var entity = new TEntity { Id = key };
        var entry = context.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            var set = GetDbSet(context);

            var found = await GetByEntityKeyAsync(context, key, false, cancellationToken);

            if (found != null)
            {
                set.Remove(found);
            }
            else
            {
                return null;
            }
        }
        else
        {
            entry.State = EntityState.Deleted;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (_publisherService is not null)
        {
            await _publisherService.PublishDeleteEventAsync(previous, transaction, cancellationToken);
        }

        return previous;
    }

    protected virtual async Task<IEnumerable<TEntity>> DeleteInternalAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        var previous = await _readService.GetAllAsync(filter, transaction, cancellationToken);

        if (previous?.IsEmpty() ?? true)
        {
            return null;
        }

        await using var context = await GetDbContextAsync(transaction, cancellationToken);
        var query = await GetFilteredQueryableAsync(context, false, cancellationToken);
        var set = context.Set<TEntity>();
        var list = await query.Where(filter).ToListAsync(cancellationToken);

        if (list.IsEmpty())
        {
            return null;
        }

        set.RemoveRange(list);

        await context.SaveChangesAsync(cancellationToken);

        foreach (var p in previous)
        {
            await _publisherService.PublishDeleteEventAsync(p, transaction, cancellationToken);
        }

        return previous;
    }

    public virtual Task<IEnumerable<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
        => DeleteInternalAsync(filter, null, cancellationToken);

    public Task<IEnumerable<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => DeleteInternalAsync(filter, entityTransaction, cancellationToken);

    public Task<TEntity> DeleteAsync(TKey filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => DeleteInternalAsync(filter, null, cancellationToken);

    public virtual Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
        => DeleteInternalAsync(key, null, cancellationToken);
}
