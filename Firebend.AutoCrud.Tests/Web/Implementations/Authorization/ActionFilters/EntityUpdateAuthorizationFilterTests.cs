using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Authorization.ActionFilters;

[TestFixture]
public class EntityUpdateAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;

    private readonly string _policy = "ResourceUpdate";
    private Mock<ActionExecutionDelegate> _nextDelegate;
    private DefaultHttpContext _defaultHttpContext;
    private Mock<ActionExecutingContext> _actionExecutingContext;
    private Mock<IEntityAuthProvider> _entityAuthProvider;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _nextDelegate = _fixture.Create<Mock<ActionExecutionDelegate>>();
        _defaultHttpContext = new DefaultHttpContext();
        _serviceProvider = new Mock<IServiceProvider>();
        _defaultHttpContext.RequestServices = _serviceProvider.Object;
        _actionContext = new Mock<ActionContext>(
            _defaultHttpContext,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            Mock.Of<ModelStateDictionary>()
        );
        _fixture.Register(() => _actionContext.Object);
        _actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        _entityAuthProvider = _fixture.Create<Mock<IEntityAuthProvider>>();
        _serviceProvider.Setup(s
                => s.GetService(typeof(IEntityAuthProvider)))
            .Returns(_entityAuthProvider.Object);
    }

    [Test]
    public void Should_Throw_If_IEntityAuthProvider_Is_Not_Set()
    {
        // given
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IEntityAuthProvider))).Returns(default);

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);

        // then
        Assert.ThrowsAsync<DependencyResolverException>(() =>
            entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
                _nextDelegate.Object));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Return_403_If_The_Body_Is_Filled_But_Authorization_Fails()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(
            AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new UpdateAuthorizationRequirement() })));

        _defaultHttpContext.Request.Method = HttpMethods.Put;

        var testEntity = _fixture.Create<ActionFilterTestHelper.TestEntity>();
        var actionArguments = new Dictionary<string, object> { { "body", testEntity } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);
        await entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        // then
        _actionExecutingContext.Object.Result.Should().NotBeNull();
        _actionExecutingContext.Object.Result.Should().BeOfType<ObjectResult>();
        _actionExecutingContext.Object.Result.As<ObjectResult>().StatusCode.Should().Be(403);
        _nextDelegate.Verify(x => x(), Times.Never);
    }


    [Test]
    public async Task Should_Next_If_The_Body_Is_Filled_And_Authorized()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(
            AuthorizationResult.Success());

        _defaultHttpContext.Request.Method = HttpMethods.Put;

        var testEntity = _fixture.Create<ActionFilterTestHelper.TestEntity>();
        var actionArguments = new Dictionary<string, object> { { "body", testEntity } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);
        await entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        // then
        _actionExecutingContext.Object.Result.Should().BeNull();
        _nextDelegate.Verify(x => x(), Times.Once);
    }

    [Test]
    public void Should_Throw_If_It_Is_Patch_And_Id_Is_Not_Provided()
    {
        // given
        _defaultHttpContext.Request.Method = HttpMethods.Patch;

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);

        // then
        Assert.ThrowsAsync<ArgumentException>(() =>
            entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
                _nextDelegate.Object));
    }

    [Test]
    public async Task Should_Return_403_If_It_Is_Patch_And_Id_Is_Not_Null_And_Authorization_Fail()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity>(
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(
            AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new UpdateAuthorizationRequirement() })));

        _defaultHttpContext.Request.Method = HttpMethods.Patch;

        var actionArguments = new Dictionary<string, object> { { "id", Guid.NewGuid().ToString() } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);

        // then
        await entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        _actionExecutingContext.Object.Result.Should().NotBeNull();
        _actionExecutingContext.Object.Result.Should().BeOfType<ObjectResult>();
        _actionExecutingContext.Object.Result.As<ObjectResult>().StatusCode.Should().Be(403);

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity>(
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Next_If_It_Is_Patch_And_Id_Is_Authorized()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity>(
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(
            AuthorizationResult.Success());

        _defaultHttpContext.Request.Method = HttpMethods.Patch;

        var actionArguments = new Dictionary<string, object> { { "id", Guid.NewGuid().ToString() } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityUpdateAuthorizationFilter =
            new EntityUpdateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity,
                ActionFilterTestHelper.TestEntity>(_policy);

        // then
        await entityUpdateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        _actionExecutingContext.Object.Result.Should().BeNull();

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity>(
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ));
        _nextDelegate.Verify(x => x(), Times.Once);
    }
}
