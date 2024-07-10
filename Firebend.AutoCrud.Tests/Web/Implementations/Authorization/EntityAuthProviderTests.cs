using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Tests.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Tests.Web.Implementations.Swagger;
using Firebend.AutoCrud.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Authorization;

[TestFixture]
public class EntityAuthProviderTests
{
    private Fixture _fixture;
    private Mock<IServiceScope> _serviceScope;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<IEntityReadService<Guid, ActionFilterTestHelper.TestEntity>> _entityReadService;
    private Mock<IEntityKeyParser<Guid, ActionFilterTestHelper.TestEntity, V1>> _entityKeyParser;
    private Mock<IAuthorizationService> _authService;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _authService = new Mock<IAuthorizationService>();
        _serviceScope = new Mock<IServiceScope>();
        _serviceProvider = new Mock<IServiceProvider>();
        _entityReadService = new Mock<IEntityReadService<Guid, ActionFilterTestHelper.TestEntity>>();

        _serviceProvider.Setup(s
                => s.GetService(typeof(IEntityReadService<Guid, ActionFilterTestHelper.TestEntity>)))
            .Returns(_entityReadService.Object);

        _serviceScope.Setup(x => x.ServiceProvider).Returns(_serviceProvider.Object);

        _entityKeyParser = new Mock<IEntityKeyParser<Guid, ActionFilterTestHelper.TestEntity, V1>>();
        _entityKeyParser.Setup(s => s.ParseKey(
            It.IsAny<string>()
        )).Returns(Guid.NewGuid());
        _serviceProvider.Setup(s
                => s.GetService(typeof(IEntityKeyParser<Guid, ActionFilterTestHelper.TestEntity, V1>)))
            .Returns(_entityKeyParser.Object);
    }

    [Test]
    public void AuthorizeEntityAsync_ByStringId_Should_Throw_If_Read_Service_Is_Not_Set()
    {
        // given
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, ActionFilterTestHelper.TestEntity>))).Returns(default);

        // when
        var entityAuthProvider =
            new DefaultEntityAuthProvider(_authService.Object, _serviceProvider.Object);

        // then
        Assert.ThrowsAsync<DependencyResolverException>(() =>
            entityAuthProvider.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(Guid.NewGuid().ToString(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _authService.Verify(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void AuthorizeEntityAsync_ByStringId_Should_Throw_If_Key_Parser_Service_Is_Not_Set()
    {
        // given
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, ActionFilterTestHelper.TestEntity, V1>))).Returns(default);

        // when
        var entityAuthProvider =
            new DefaultEntityAuthProvider(_authService.Object, _serviceProvider.Object);

        // then
        Assert.ThrowsAsync<DependencyResolverException>(() =>
            entityAuthProvider.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(Guid.NewGuid().ToString(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _authService.Verify(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void AuthorizeEntityAsync_ByStringId_Should_Throw_If_Key_Cannot_Be_Parsed()
    {
        // given
        _entityKeyParser.Setup(s
            => s.ParseKey(It.IsAny<string>())).Returns((Guid?)null);

        // when
        var entityAuthProvider =
            new DefaultEntityAuthProvider(_authService.Object, _serviceProvider.Object);

        // then
        Assert.ThrowsAsync<ArgumentException>(() =>
            entityAuthProvider.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(Guid.NewGuid().ToString(),
                It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        _authService.Verify(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task AuthorizeEntityAsync_ByStringId_Should_Call_Authorization_Service_With_Entity()
    {
        // given

        // when
        var entityAuthProvider =
            new DefaultEntityAuthProvider(_authService.Object, _serviceProvider.Object);

        // then
        await entityAuthProvider.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
            Guid.NewGuid().ToString(),
            It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<CancellationToken>());

        _entityKeyParser.Verify(p => p.ParseKey(It.IsAny<string>()));
        _entityReadService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
        _authService.Verify(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
    }

    [Test]
    public async Task AuthorizeEntityAsync_ById_Should_Call_Authorization_Service_With_Entity()
    {
        // given

        // when
        var entityAuthProvider =
            new DefaultEntityAuthProvider(_authService.Object, _serviceProvider.Object);

        // then
        await entityAuthProvider.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
            It.IsAny<Guid>(),
            It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<CancellationToken>());

        _entityKeyParser.Verify(p => p.ParseKey(It.IsAny<string>()), Times.Never);
        _entityReadService.Verify(s => s.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()));
        _authService.Verify(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
    }
}
