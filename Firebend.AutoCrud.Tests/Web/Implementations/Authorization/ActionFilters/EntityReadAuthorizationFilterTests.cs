using System;
using System.Security.Claims;
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
public class EntityReadAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<HttpContext> _httpContext;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;

    private readonly string _policy = "ResourceRead";
    private Mock<ResultExecutionDelegate> _nextDelegate;
    private Mock<ResultExecutingContext> _resultExecutingContext;
    private Mock<IEntityAuthProvider> _entityAuthProvider;

    [SetUp]
    public void SetUpFixture()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());
        _nextDelegate = _fixture.Create<Mock<ResultExecutionDelegate>>();
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
        _resultExecutingContext = _fixture.Create<Mock<ResultExecutingContext>>();

        _entityAuthProvider = _fixture.Create<Mock<IEntityAuthProvider>>();
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

        var entityReadAuthorizationFilter =
            new EntityReadAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity>(_policy);

        // when
        Assert.Throws<DependencyResolverException>(() =>
            entityReadAuthorizationFilter.OnResultExecutionAsync(_resultExecutingContext.Object, default));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Next_If_Context_Result_Is_Not_Ok_Object_Result()
    {
        // given
        _resultExecutingContext.Setup(s => s.Result).Returns(new Mock<NotFoundResult>().Object);

        var entityReadAuthorizationFilter =
            new EntityReadAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity>(_policy);

        // when

        await entityReadAuthorizationFilter.OnResultExecutionAsync(_resultExecutingContext.Object,
            _nextDelegate.Object);
        _nextDelegate.Verify(x => x(), Times.Once);
    }

    [Test]
    public void Should_Return_403_If_Authorization_Fails()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(
            AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] {new ReadAuthorizationRequirement()})));

        var okObjectResult = _fixture.Create<OkObjectResult>();
        _resultExecutingContext.Object.Result = okObjectResult;

        var entityReadAuthorizationFilter =
            new EntityReadAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity>(_policy);

        // when
        entityReadAuthorizationFilter.OnResultExecutionAsync(_resultExecutingContext.Object, _nextDelegate.Object);

        // then
        _resultExecutingContext.Object.Result.Should().NotBeNull();
        _resultExecutingContext.Object.Result.Should().BeOfType<ObjectResult>();
        _resultExecutingContext.Object.Result.As<ObjectResult>().StatusCode.Should().Be(403);

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
        _nextDelegate.Verify(x => x(), Times.Once);
    }

    [Test]
    public async Task Should_Next_If_Authorized()
    {
        // given

        var entityReadAuthorizationFilter =
            new EntityReadAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity>(_policy);

        // when
        await entityReadAuthorizationFilter.OnResultExecutionAsync(_resultExecutingContext.Object,
            _nextDelegate.Object);
        _nextDelegate.Verify(x => x(), Times.Once);
    }
}
