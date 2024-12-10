using System;
using System.Text.Json;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Firebend.AutoCrud.Tests.Caching;

public class TestEntity : IEntity<int>
{
    public int Id { get; set; }
}

public class TestSerializer : IEntityCacheSerializer
{
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value);
    public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
}

public class TestEntityCacheOptions : IEntityCacheOptions
{
    public IEntityCacheSerializer Serializer => new TestSerializer();

    public DistributedCacheEntryOptions GetCacheEntryOptions<TEntity>(TEntity entity) =>
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };

    public string CollectionKey => "TestEntity";
    public int MaxCollectionSize => 10;

    public string GetKey<TKey>(TKey key) where TKey : struct => $"TestEntity:{key}";
}
