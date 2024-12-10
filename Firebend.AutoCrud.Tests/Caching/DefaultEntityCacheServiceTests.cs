using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Caching;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Caching;

public class TestEntity : IEntity<int>
{
    public int Id { get; set; }
}

[TestFixture]
public class DefaultEntityCacheServiceTests
{
    private Fixture _fixture;
    private MemoryDistributedCache _memoryCache;
    private EntityCacheOptions _cacheOptions;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _cacheOptions = new EntityCacheOptions();
        _memoryCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

        _fixture.Inject<IEntityCacheOptions>(_cacheOptions);
    }

    [Test]
    public async Task EntityCacheService_Should_Set_Entity()
    {
        // Arrange
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        await sut.SetAsync(testEntity);

        // Assert
        var cached = await _memoryCache.GetAsync("TestEntity:1");
        cached.Should().NotBeNull();
    }

    [Test]
    public async Task EntityCacheService_Should_Use_Prefix()
    {
        // Arrange
        _cacheOptions.CacheKeyPrefix = () => "TestPrefix";
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        await sut.SetAsync(testEntity);

        // Assert
        var cached = await _memoryCache.GetAsync("TestPrefix:TestEntity:1");
        cached.Should().NotBeNull();
    }

    [Test]
    public async Task EntityCacheService_Should_Get_Entity()
    {
        // Arrange
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();
        await sut.SetAsync(testEntity);

        // Act
        var cached = await sut.GetAsync(1);

        // Assert
        cached.Should().NotBeNull();
        cached!.Id.Should().Be(testEntity.Id);
    }

    [Test]
    public async Task EntityCacheService_Should_Remove_Entity()
    {
        // Arrange
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();
        await sut.SetAsync(testEntity);

        // Act
        await sut.RemoveAsync(1);

        // Assert
        var cached = await _memoryCache.GetAsync("TestEntity:1");
        cached.Should().BeNull();
    }

    [Test]
    public void EntityCacheService_Should_ThrowError_When_CacheKey_Is_NullOrDefault()
    {
        // Arrange
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        async Task ActionDefault() => await sut.GetAsync(default);

        // Assert
        Assert.ThrowsAsync<ArgumentNullException>(ActionDefault);
    }

    [Test]
    public async Task EntityCacheService_Should_CatchAndLogException_When_SetAsync_Fails()
    {
        // Arrange
        _fixture.Freeze<Mock<IDistributedCache>>().Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), default)).Throws<Exception>();

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        await sut.SetAsync(testEntity);

        // Assert
        // No exception should be thrown
    }

    [Test]
    public async Task EntityCacheService_Should_CatchAndLogException_When_RemoveAsync_Fails()
    {
        // Arrange
        _fixture.Freeze<Mock<IDistributedCache>>().Setup(x => x.RemoveAsync(It.IsAny<string>(), default))
            .Throws<Exception>();

        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        await sut.RemoveAsync(1);

        // Assert
        // No exception should be thrown
    }

    [Test]
    public async Task EntityCacheService_Should_Return_Null_When_GetAsync_Fails()
    {
        // Arrange
        _fixture.Freeze<Mock<IDistributedCache>>().Setup(x => x.GetAsync(It.IsAny<string>(), default))
            .Throws<Exception>();

        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        var result = await sut.GetAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task EntityCacheServiceExtensions_Should_GetOrSetAsync()
    {
        // Arrange
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();

        // Act
        var result = await sut.GetOrSetAsync(1, () => Task.FromResult(testEntity), default);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testEntity.Id);
    }

    [Test]
    public async Task EntityCacheServiceExtensions_Should_GetOrSetAsync_When_Entity_Is_Cached()
    {
        // Arrange
        _fixture.Inject<IDistributedCache>(_memoryCache);

        var testEntity = new TestEntity { Id = 1 };
        var sut = _fixture.Create<DefaultEntityCacheService<int, TestEntity>>();
        await sut.SetAsync(testEntity);

        // Act
        var result = await sut.GetOrSetAsync(1, () => Task.FromResult(new TestEntity { Id = 2 }), default);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testEntity.Id);
    }
}
