

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.CustomFields.Web.Implementations.Authorization;
using Firebend.AutoCrud.Tests.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Tests.Web.Implementations.Swagger;
using Firebend.AutoCrud.Web.Implementations.Authorization;
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

namespace Firebend.AutoCrud.Tests.CustomFields.Web.Implementations.Authorization;

[TestFixture]
public class CustomFieldsAuthorizationFilterTests
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
    public void Should_Throw_If_Authorization_Service_Is_Not_Set()
    {
        // given
        _serviceProvider.Setup(s =>
            s.GetService(typeof(IEntityAuthProvider))).Returns(default);

        // when
        var CustomFieldsAuthorizationFilter =
            new CustomFieldsAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1>(_policy);

        // then
        Assert.ThrowsAsync<DependencyResolverException>(() =>
            CustomFieldsAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
                _nextDelegate.Object));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public void Should_Throw_If_Id_Is_Null()
    {
        // given

        // when
        var CustomFieldsAuthorizationFilter =
            new CustomFieldsAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1>(_policy);

        // then
        Assert.ThrowsAsync<ArgumentException>(() =>
            CustomFieldsAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
                _nextDelegate.Object));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Return_403_If_Id_Is_Not_Null_And_Authorization_Fail()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(
            AuthorizationResult.Failed(
                AuthorizationFailure.Failed([new CustomFieldsAuthorizationRequirement()])));

        var actionArguments = new Dictionary<string, object> { { "entityId", Guid.NewGuid().ToString() } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var CustomFieldsAuthorizationFilter =
            new CustomFieldsAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1>(_policy);

        // then
        await CustomFieldsAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        _actionExecutingContext.Object.Result.Should().NotBeNull();
        _actionExecutingContext.Object.Result.Should().BeOfType<ObjectResult>();
        _actionExecutingContext.Object.Result.As<ObjectResult>().StatusCode.Should().Be(403);

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ));
        _nextDelegate.Verify(x => x(), Times.Never);
    }

    [Test]
    public async Task Should_Next_If_Id_Is_Not_Null_And_Authorization_Succeeds()
    {
        // given
        _entityAuthProvider.Setup(a => a.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(
            AuthorizationResult.Success());

        var actionArguments = new Dictionary<string, object> { { "entityId", Guid.NewGuid().ToString() } };
        _actionExecutingContext.Setup(a => a.ActionArguments).Returns(actionArguments);

        // when
        var CustomFieldsAuthorizationFilter =
            new CustomFieldsAuthorizationFilter<Guid, ActionFilterTestHelper.TestEntity, V1>(_policy);

        // then
        await CustomFieldsAuthorizationFilter.OnActionExecutionAsync(_actionExecutingContext.Object,
            _nextDelegate.Object);

        _actionExecutingContext.Object.Result.Should().BeNull();

        _entityAuthProvider.Verify(v =>
            v.AuthorizeEntityAsync<Guid, ActionFilterTestHelper.TestEntity, V1>(
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ));
        _nextDelegate.Verify(x => x(), Times.Once);
    }
}
