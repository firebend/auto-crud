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
public class EntityReadAllAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<HttpContext> _httpContext;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;

    private string _policy = "ResourceReadAll";

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

        var resultExecutingContext = _fixture.Create<Mock<ResultExecutingContext>>();

        Task<ResultExecutedContext> Next()
        {
            // then
            Assert.Pass();
            var ctx = new ResultExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<IActionResult>(),Mock.Of<Controller>());
            return Task.FromResult(ctx);
        }

        var entityReadAllAuthorizationFilter = new EntityReadAllAuthorizationFilter(_policy);

        // when
        entityReadAllAuthorizationFilter.OnResultExecutionAsync(resultExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Context_Result_Is_Not_Ok_Object_Result()
    {
        // given
        var authorizationService = new Mock<IAuthorizationService>();

        _serviceProvider.Setup(s
                => s.GetService(typeof(IAuthorizationService)))
            .Returns(authorizationService.Object);

        var resultExecutingContext = _fixture.Create<Mock<ResultExecutingContext>>();

        resultExecutingContext.Setup(s => s.Result).Returns(new Mock<NotFoundResult>().Object);

        var entityReadAllAuthorizationFilter = new EntityReadAllAuthorizationFilter(_policy);

        // when

        Task<ResultExecutedContext> Next()
        {
            // then
            Assert.Pass();
            var ctx = new ResultExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<IActionResult>(),Mock.Of<Controller>());
            return Task.FromResult(ctx);
        }

        entityReadAllAuthorizationFilter.OnResultExecutionAsync(resultExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Return_403_If_Authorization_Fails()
    {
        // given
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new []{ new ReadAllAuthorizationRequirement()})));

        _serviceProvider.Setup(s
                => s.GetService(typeof(IAuthorizationService)))
            .Returns(authorizationService.Object);

        var resultExecutingContext = _fixture.Create<Mock<ResultExecutingContext>>();
        var okObjectResult = _fixture.Create<OkObjectResult>();
        resultExecutingContext.Object.Result = okObjectResult;

        var entityReadAllAuthorizationFilter = new EntityReadAllAuthorizationFilter(_policy);

        // when
        entityReadAllAuthorizationFilter.OnResultExecutionAsync(resultExecutingContext.Object, It.IsAny<ResultExecutionDelegate>());

        // then
        resultExecutingContext.Object.Result.Should().NotBeNull();
        resultExecutingContext.Object.Result.Should().BeOfType<StatusCodeResult>();
        resultExecutingContext.Object.Result.As<StatusCodeResult>().StatusCode.Should().Be(403);
    }

}
