using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Extensions;

public static class EntityCacheServiceExtensions
{
    public static async Task<TEntity> GetOrSetAsync<TKey, TEntity>(
        this IEntityCacheService<TKey, TEntity> cacheService,
        TKey key,
        Func<Task<TEntity>> factory,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var entity = await cacheService.GetAsync(key, cancellationToken);

        if (entity is not null)
        {
            return entity;
        }

        entity = await factory();

        if (entity is not null)
        {
            await cacheService.SetAsync(entity, cancellationToken);
        }

        return entity;
    }

    public static async Task<List<TEntity>> GetOrSetAsync<TKey, TEntity>(
        this IEntityCacheService<TKey, TEntity> cacheService,
        Func<Task<List<TEntity>>> factory,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var collection = await cacheService.GetCollectionAsync(cancellationToken);

        if (collection is not null)
        {
            return collection;
        }

        collection = await factory();

        if (collection.HasValues())
        {
            await cacheService.SetCollectionAsync(collection, cancellationToken);
        }

        return collection;
    }
}
