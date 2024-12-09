using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Caching.interfaces;

public interface IEntityCacheOptions
{
    public IEntityCacheSerializer Serializer { get; }
    public DistributedCacheEntryOptions GetCacheEntryOptions<TEntity>(TEntity entity);
    public string GetKey<TKey>(TKey key) where TKey : struct;
}
