using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
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
public class EntityCreateMultipleAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<HttpContext> _httpContext;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;

    private string _policy = "ResourceCreateMultiple";

    [SetUp]
    public void SetUpFixture()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

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
    }

    [Test]
    public void Should_Execute_Next_If_An_Authorization_Service_Not_Set()
    {
        // given
        _serviceProvider.Setup(s
            => s.GetService(typeof(IAuthorizationService))).Returns(default);

        var actionExecutingContext = new ActionExecutingContext(
            _actionContext.Object,
            Mock.Of<List<IFilterMetadata>>(),
            Mock.Of<IDictionary<string,object>>(),
            Mock.Of<Controller>()
        );

        Task<ActionExecutedContext> Next()
        {
            // then
            Assert.Pass();
            var ctx = new ActionExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<Controller>());
            return Task.FromResult(ctx);
        }

        // when
        var entityCreateMultipleAuthorizationFilter = new EntityCreateMultipleAuthorizationFilter<EntityCreateMultipleAuthorizationFilterTestClass>(_policy);
        entityCreateMultipleAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext, Next);
    }

    [Test]
    public void Should_Return_Status_Forbidden_If_Authorization_Fails()
    {
        // given
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new []{ new CreateMultipleAuthorizationRequirement()})));

        _serviceProvider.Setup(s
                => s.GetService(typeof(IAuthorizationService)))
            .Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        var postObject = _fixture.Create<EntityCreateMultipleAuthorizationFilterTestClass>();
        var actionArguments = new Dictionary<string, object> {{"body", postObject}};
        actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityCreateMultipleAuthorizationFilter = new EntityCreateMultipleAuthorizationFilter<EntityCreateMultipleAuthorizationFilterTestClass>(_policy);
        entityCreateMultipleAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, It.IsAny<ActionExecutionDelegate>());

        // then
        actionExecutingContext.Object.Result.Should().NotBeNull();
        actionExecutingContext.Object.Result.Should().BeOfType<StatusCodeResult>();
        actionExecutingContext.Object.Result.As<StatusCodeResult>().StatusCode.Should().Be(403);
    }

    [Test]
    public void Should_Next_If_Body_Is_Empty()
    {
        // given
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new []{ new CreateMultipleAuthorizationRequirement()})));

        _serviceProvider.Setup(s
                => s.GetService(typeof(IAuthorizationService)))
            .Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityCreateMultipleAuthorizationFilter = new EntityCreateMultipleAuthorizationFilter<EntityCreateMultipleAuthorizationFilterTestClass>(_policy);

        Task<ActionExecutedContext> Next()
        {
            // then
            Assert.Pass();
            var ctx = new ActionExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<Controller>());
            return Task.FromResult(ctx);
        }

        entityCreateMultipleAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }
}

public class EntityCreateMultipleAuthorizationFilterTestClass
{
    public int Id { get; set; }
    public string Name { get; set; }
}
