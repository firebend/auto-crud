using System;
using Firebend.AutoCrud.Caching.extensions;
using Firebend.AutoCrud.Caching.interfaces;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Caching;

[TestFixture]
public class EntityCachingExtensionsTests
{
    [Test]
    public void WithEntityCaching_Should_ThrowError_When_IDistributedCache_Is_NotRegistered()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();

        // Act
        void Action() => services.WithEntityCaching<TestEntityCacheOptions>();

        // Assert
        Assert.Throws<InvalidOperationException>(Action);
    }

    [Test]
    public void AddEntityCache_Should_ThrowError_When_WithEntityCaching_Is_NotCalled()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();
        services.AddDistributedMemoryCache();

        // Act
        void Action() => services.AddEntityCache<int, TestEntity>();

        // Assert
        Assert.Throws<InvalidOperationException>(Action);
    }

    [Test]
    public void AddEntityCache_Should_Register_EntityCacheService()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();
        services.AddDistributedMemoryCache();
        services.WithEntityCaching<TestEntityCacheOptions>();

        // Act
        services.AddEntityCache<int, TestEntity>();

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IEntityCacheService<int, TestEntity>>();
        Assert.That(service is not null);
    }

}
