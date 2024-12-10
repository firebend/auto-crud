using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheService<in TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken);
    public Task SetAsync(TEntity entity, CancellationToken cancellationToken);
    public Task SetAsync(TEntity entity, DistributedCacheEntryOptions cacheOptions, CancellationToken cancellationToken);
    public Task RemoveAsync(TKey key, CancellationToken cancellationToken);
    public Task<List<TEntity>> GetCollectionAsync(CancellationToken cancellationToken);
    public Task SetCollectionAsync(List<TEntity> entities, CancellationToken cancellationToken);
    public Task SetCollectionAsync(List<TEntity> entities, DistributedCacheEntryOptions cacheOptions, CancellationToken cancellationToken);
    public Task RemoveCollectionAsync(CancellationToken cancellationToken);
}
