using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
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

namespace Firebend.AutoCrud.Tests.Web.Implementations.Authorization.ActionFilters;

[TestFixture]
public class EntityDeleteAuthorizationFilterTests
{
    private Fixture _fixture;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<ActionContext> _actionContext;
    private DefaultHttpContext _defaultHttpContext;

    private readonly string _policy = "ResourceDelete";

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
        var entityDeleteAuthorizationFilter = new EntityDeleteAuthorizationFilter<Guid, DeleteEntityTest>(_policy);

        // then
        entityDeleteAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Key_Parser_Service_Is_Not_Set()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, DeleteEntityTest>))).Returns(entityReadService.Object);

        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<,>))).Returns(default);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var deleteAuthorizationFilter = new EntityDeleteAuthorizationFilter<Guid, DeleteEntityTest>(_policy);

        // then
        deleteAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Authorization_Service_Is_Not_Set()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, DeleteEntityTest>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, DeleteEntityTest>))).Returns(entityKeyParser.Object);

        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(default);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityDeleteAuthorizationFilter = new EntityDeleteAuthorizationFilter<Guid, DeleteEntityTest>(_policy);

        // then
        entityDeleteAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Next_If_Id_Is_Not_Set()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, DeleteEntityTest>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, DeleteEntityTest>))).Returns(entityKeyParser.Object);

        var authorizationService = new Mock<IAuthorizationService>();
        _serviceProvider.Setup(s =>
             s.GetService(typeof(IAuthorizationService))).Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        // when
        var entityDeleteAuthorizationFilter =
            new EntityDeleteAuthorizationFilter<Guid, DeleteEntityTest>(_policy);

        // then
        entityDeleteAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);
    }

    [Test]
    public void Should_Return_403_If_Id_Is_Not_Null_And_Authorization_Fails()
    {
        // given
        var entityReadService = new Mock<IEntityReadService<Guid, DeleteEntityTest>>();
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityReadService<Guid, DeleteEntityTest>))).Returns(entityReadService.Object);

        var entityKeyParser = new Mock<IEntityKeyParser<Guid, DeleteEntityTest>>();
        entityKeyParser.Setup(s => s.ParseKey(
                It.IsAny<string>()
            )).Returns(Guid.NewGuid());
        _serviceProvider.Setup(s
            => s.GetService(typeof(IEntityKeyParser<Guid, DeleteEntityTest>))).Returns(entityKeyParser.Object);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
        )).ReturnsAsync(AuthorizationResult.Failed(AuthorizationFailure.Failed(new[] { new UpdateAuthorizationRequirement() })));
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IAuthorizationService))).Returns(authorizationService.Object);

        var actionExecutingContext = _fixture.Create<Mock<ActionExecutingContext>>();

        var actionArguments = new Dictionary<string, object> { { "id", Guid.NewGuid().ToString() } };
        actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var entityDeleteAuthorizationFilter =
            new EntityDeleteAuthorizationFilter<Guid, DeleteEntityTest>(_policy);

        // then
        entityDeleteAuthorizationFilter.OnActionExecutionAsync(actionExecutingContext.Object, Next);

        actionExecutingContext.Object.Result.Should().NotBeNull();
        actionExecutingContext.Object.Result.Should().BeOfType<StatusCodeResult>();
        actionExecutingContext.Object.Result.As<StatusCodeResult>().StatusCode.Should().Be(403);

        entityReadService.Verify(v => v.GetByKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        entityKeyParser.Verify(v => v.ParseKey(It.IsAny<string>()), Times.Once);
        authorizationService.Verify(v => v.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()));
    }

    private Task<ActionExecutedContext> Next()
    {
        // then
        Assert.Pass();
        var ctx = new ActionExecutedContext(_actionContext.Object, Mock.Of<List<IFilterMetadata>>(), Mock.Of<Controller>());
        return Task.FromResult(ctx);
    }
}


public class DeleteEntityTest : IEntity<Guid>
{
    public string WhoAreYou { get; set; }
    public bool AreYouHavingFun { get; set; }
    public DateTime LastTimeYouHadFun { get; set; }
    public Guid Id { get; set; }
}
