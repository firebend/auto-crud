using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Tests.Web.Implementations.Swagger;
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
public class EntityCreateAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<HttpContext> _httpContext;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;

    private readonly string _policy = "ResourceCreate";
    private Mock<ActionExecutionDelegate> _nextDelegate;
    private Mock<IEntityAuthProvider> _entityAuthProvider;
    private Mock<ActionExecutingContext> _actionExecutingContext;

    [SetUp]
    public void SetUpFixture()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _nextDelegate = _fixture.Create<Mock<ActionExecutionDelegate>>();
        _entityAuthProvider = _fixture.Create<Mock<IEntityAuthProvider>>();
        _httpContext = new Mock<HttpContext>();
        _serviceProvider = new Mock<IServiceProvider>();
        _httpContext.SetupProperty(s
            => s.RequestServices, _serviceProvider.Object);
        _actionContext = new Mock<ActionContext>(
            _httpContext.Object,
            Mock.Of<RouteData>(),
            Mock.Of<ActionDescriptor>(),
            Mock.Of<ModelStateDictionary>()
        );
        _fixture.Register(() => _actionContext.Object);
        _actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();
        _serviceProvider.Setup(s
                => s.GetService(typeof(IEntityAuthProvider)))
            .Returns(_entityAuthProvider.Object);
    }

    [Test]
    public void Should_Throw_If_EntityAuthProvider_Not_Registered()
    {
        // given
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityAuthProvider))).Returns(default);

        // when
        var entityCreateAuthorizationFilter =
            new EntityCreateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1,
                ActionFilterTestHelper.TestEntity>(_policy);
        Assert.ThrowsAsync<DependencyResolverException>(() =>
            entityCreateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object, _nextDelegate.Object));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Return_Status_Forbidden_If_Authorization_Fails()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(
            AuthorizationResult.Failed(AuthorizationFailure.Failed([new CreateAuthorizationRequirement()])));

        var postObject = _fixture.Create<ActionFilterTestHelper.TestEntity>();
        var actionArguments = new Dictionary<string, object> { { "body", postObject } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityCreateAuthorizationFilter =
            new EntityCreateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1,
                ActionFilterTestHelper.TestEntity>(_policy);

        await entityCreateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        // then
        _actionExecutingContext.Object.Result.Should().NotBeNull();
        _actionExecutingContext.Object.Result.Should().BeOfType<ObjectResult>();
        _actionExecutingContext.Object.Result.As<ObjectResult>().StatusCode.Should().Be(403);

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Next_If_Authorized()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(
            AuthorizationResult.Success());

        var postObject = _fixture.Create<ActionFilterTestHelper.TestEntity>();
        var actionArguments = new Dictionary<string, object> { { "body", postObject } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityCreateAuthorizationFilter =
            new EntityCreateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1,
                ActionFilterTestHelper.TestEntity>(_policy);
        await entityCreateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        // then
        _actionExecutingContext.Object.Result.Should().BeNull();

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
        _nextDelegate.Verify(x => x(), Times.Once);
    }

    [Test]
    public void Should_Throw_Exception_If_Cannot_Resolve_Body()
    {
        // given

        // when
        var entityCreateAuthorizationFilter =
            new EntityCreateAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1,
                ActionFilterTestHelper.TestEntity>(_policy);

        Assert.ThrowsAsync<ArgumentException>(() =>
            entityCreateAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
                _nextDelegate.Object));

        _nextDelegate.Verify(x => x(), Times.Never);
    }
}
