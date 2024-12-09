using Firebend.AutoCrud.Caching.interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Caching.extensions;

public static class EntityCacheServiceExtensions
{
    public static async Task<TEntity?> GetOrCreateAsync<TKey, TEntity>(
        this IEntityCacheService<TKey, TEntity> cacheService,
        TKey key,
        Func<Task<TEntity?>> factory,
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
}
