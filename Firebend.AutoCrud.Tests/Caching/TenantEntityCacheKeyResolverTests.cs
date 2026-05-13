using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations.Caching;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Caching;

[TestFixture]
public class TenantEntityCacheKeyResolverTests
{
    [Test]
    public async Task TenantEntityCacheKeyResolver_Should_Return_Tenant_Id_Segment()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new Mock<ITenantEntityProvider<Guid>>();
        tenantProvider.Setup(x => x.GetTenantAsync(default))
            .ReturnsAsync(new TenantEntityResult<Guid> { TenantId = tenantId });

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITenantEntityProvider<Guid>)))
            .Returns(tenantProvider.Object);

        var sut = new TenantEntityCacheKeyResolver(serviceProvider.Object);

        var result = await sut.GetTenantIdSegmentAsync(typeof(TestTenantEntity), default);

        result.Should().Be($"{tenantId}");
    }

    [Test]
    public void TenantEntityCacheKeyResolver_Should_Throw_When_Provider_Is_Not_Registered()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var sut = new TenantEntityCacheKeyResolver(serviceProvider.Object);

        async Task Action() => await sut.GetTenantIdSegmentAsync(typeof(TestTenantEntity), default);

        Assert.ThrowsAsync<InvalidOperationException>(Action);
    }

    [Test]
    public void TenantEntityCacheKeyResolver_Should_Throw_When_Tenant_Id_Is_Default()
    {
        var tenantProvider = new Mock<ITenantEntityProvider<Guid>>();
        tenantProvider.Setup(x => x.GetTenantAsync(default))
            .ReturnsAsync(new TenantEntityResult<Guid>());

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITenantEntityProvider<Guid>)))
            .Returns(tenantProvider.Object);

        var sut = new TenantEntityCacheKeyResolver(serviceProvider.Object);

        async Task Action() => await sut.GetTenantIdSegmentAsync(typeof(TestTenantEntity), default);

        Assert.ThrowsAsync<InvalidOperationException>(Action);
    }

    [Test]
    public void TenantEntityCacheKeyResolver_Should_Throw_When_Tenant_Is_Null()
    {
        var tenantProvider = new Mock<ITenantEntityProvider<Guid>>();
        tenantProvider.Setup(x => x.GetTenantAsync(default))
            .ReturnsAsync((TenantEntityResult<Guid>)null!);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITenantEntityProvider<Guid>)))
            .Returns(tenantProvider.Object);

        var sut = new TenantEntityCacheKeyResolver(serviceProvider.Object);

        async Task Action() => await sut.GetTenantIdSegmentAsync(typeof(TestTenantEntity), default);

        Assert.ThrowsAsync<InvalidOperationException>(Action);
    }
}
