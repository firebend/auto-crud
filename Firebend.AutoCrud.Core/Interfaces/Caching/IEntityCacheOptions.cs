using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheOptions
{
    public IEntityCacheSerializer Serializer { get; }
    public DistributedCacheEntryOptions GetCacheEntryOptions<TEntity>(TEntity entity);
    public int MaxCollectionSize { get; }
}
