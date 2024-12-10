using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Caching;

[TestFixture]
public class EntityCachingExtensionsTests
{
    private class TestBuilder : EntityCrudBuilder<int, TestEntity>
    {
        public TestBuilder(IServiceCollection services) : base(services)
        {
        }

        public override Type CreateType { get; }
        public override Type ReadType { get; }
        public override Type SearchType { get; }
        public override Type UpdateType { get; }
        public override Type DeleteType { get; }
        protected override void ApplyPlatformTypes() => throw new NotImplementedException();
    }

    [Test]
    public void WithEntityCaching_Should_ThrowError_When_IDistributedCache_Is_NotRegistered()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();

        // Act
        void Action() => services.WithEntityCaching();

        // Assert
        Assert.Throws<InvalidOperationException>(Action);
    }

    [Test]
    public void WithEntityCaching_Should_ConfigureDefaultEntityCacheOptions()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddDistributedMemoryCache();
        var builder = new TestBuilder(services);

        // Act
        services.WithEntityCaching(o => o.MaxCollectionSize = 10);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IEntityCacheOptions>();
        options.Should().NotBeNull();
        options!.MaxCollectionSize.Should().Be(10);
    }

    [Test]
    public void AddEntityCaching_Should_ThrowError_When_WithEntityCaching_Is_NotCalled()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddDistributedMemoryCache();
        var builder = new TestBuilder(services);
        services.AddDistributedMemoryCache();

        // Act
        void Action() => builder.AddEntityCaching();

        // Assert
        Assert.Throws<InvalidOperationException>(Action);
    }

    [Test]
    public void AddEntityCaching_Should_Register_EntityCacheService()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddDistributedMemoryCache()
            .WithEntityCaching();
        var builder = new TestBuilder(services);

        // Act
        builder.AddEntityCaching();

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IEntityCacheService<int, TestEntity>>();
        Assert.That(service is not null);
    }
}
