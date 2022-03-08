using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
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

namespace Firebend.AutoCrud.Tests.ChangeTracking.Web.Authorization;

[TestFixture]
public class EntityChangeTrackingAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;
    private DefaultHttpContext _defaultHttpContext;

    private readonly string _policy = "ResourceChangeTracking";

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

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
    }

    [Test]
    public void Should_Next_If_Read_Service_Is_Not_Set()
    {
        // given
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<,>))).Returns(default);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityChangeTrackingAuthorizationFilter = new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Key_Parser_Service_Is_Not_Set()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityReadService.Object);

        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<,>))).Returns(default);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityChangeTrackingAuthorizationFilter = new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Authorization_Service_Is_Not_Set()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityKeyParser.Object);

        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(default);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityChangeTrackingAuthorizationFilter = new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Id_Is_Null()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityKeyParser.Object);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new ChangeTrackingAuthorizationRequirement() })));
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityChangeTrackingAuthorizationFilter =
            new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Key_Parser_Fails_To_Parse()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityKeyParser.Object);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new ChangeTrackingAuthorizationRequirement() })));
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        var actionArguments = new Dictionary<string, object> { { "EntityId", Guid.NewGuid().ToString() } };
        actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityChangeTrackingAuthorizationFilter =
            new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Return_403_If_Id_Is_Not_Null_And_Authorization_Fail()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>>();
        entityKeyParser.Setup(s
            => s.ParseKey(It.IsAny<string>()))
            .Returns(Guid.NewGuid());
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, EntityChangeTrackingAuthorizationTestClass>))).Returns(entityKeyParser.Object);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new ChangeTrackingAuthorizationRequirement() })));
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        var actionArguments = new Dictionary<string, object> { { "EntityId", Guid.NewGuid().ToString() } };
        actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityChangeTrackingAuthorizationFilter =
            new EntityChangeTrackingAuthorizationFilter<Guid, EntityChangeTrackingAuthorizationTestClass>(_policy);

        // then
        entityChangeTrackingAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, default);

        actionExecutingContext.Object.Result.Should().NotBeNull();
        actionExecutingContext.Object.Result.Should().BeOfType<StatusCodeResult>();
        actionExecutingContext.Object.Result.As<StatusCodeResult>().StatusCode.Should().Be(403);
    }

    private Task<ActionExecutedContext> Next()
    {
        // then
        Assert.Pass();
        var ctx = new ActionExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<Controller>());
        return Task.FromResult(ctx);
    }

}

public class EntityChangeTrackingAuthorizationTestClass : IEntity<Guid>
{
    public Guid Id { get; set; }
}
