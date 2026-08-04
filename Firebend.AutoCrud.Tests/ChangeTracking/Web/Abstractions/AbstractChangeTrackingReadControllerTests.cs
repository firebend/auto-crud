using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Web.Interfaces;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.Web.Abstractions;

public class ControllerTestVersion : IAutoCrudApiVersion
{
    public int Version => 1;
    public string Name => "Version 1";
}

public class ControllerTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class ControllerTestViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class ControllerCustomRow : ChangeTrackingEntity<Guid, ControllerTestEntity>
{
    public string RealActorEmail { get; set; }
}

public class ControllerCustomViewModel : ChangeTrackingModel<Guid, ControllerTestViewModel>
{
    public string RealActorEmail { get; set; }
}

public class TestChangeTrackingReadController :
    AbstractChangeTrackingReadController<Guid, ControllerTestEntity, ControllerTestVersion, ControllerTestViewModel, ControllerCustomRow, ControllerCustomViewModel>
{
    public TestChangeTrackingReadController(
        IEntityKeyParser<Guid, ControllerTestEntity, ControllerTestVersion> keyParser,
        IOptions<ApiBehaviorOptions> apiOptions,
        IChangeTrackingReadService<Guid, ControllerTestEntity, ControllerCustomRow> read,
        IMaxPageSize<Guid, ControllerTestEntity, ControllerTestVersion> maxPageSize,
        IChangeTrackingViewModelMapper<Guid, ControllerTestEntity, ControllerTestVersion, ControllerTestViewModel, ControllerCustomRow, ControllerCustomViewModel> mapper)
        : base(keyParser, apiOptions, read, maxPageSize, mapper)
    {
    }
}

[TestFixture]
public class AbstractChangeTrackingReadControllerTests
{
    [Test]
    public async Task GetChangesAsync_WithCustomRowType_ReturnsMappedViewModelWithExtraColumn()
    {
        var entityId = Guid.NewGuid();

        var keyParser = new Mock<IEntityKeyParser<Guid, ControllerTestEntity, ControllerTestVersion>>();
        keyParser.Setup(x => x.ParseKey(entityId.ToString())).Returns(entityId);

        var row = new ControllerCustomRow { Id = Guid.NewGuid(), EntityId = entityId, RealActorEmail = "actor@test.com" };
        var pagedResponse = new EntityPagedResponse<ControllerCustomRow>
        {
            Data = [row],
            TotalRecords = 1,
            CurrentPage = 1,
            CurrentPageSize = 1
        };

        var readService = new Mock<IChangeTrackingReadService<Guid, ControllerTestEntity, ControllerCustomRow>>();
        readService.Setup(x => x.GetChangesByEntityId(It.IsAny<ChangeTrackingSearchRequest<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        var mappedViewModel = new ControllerCustomViewModel { EntityId = entityId, RealActorEmail = "actor@test.com" };
        var mapper = new Mock<IChangeTrackingViewModelMapper<Guid, ControllerTestEntity, ControllerTestVersion, ControllerTestViewModel, ControllerCustomRow, ControllerCustomViewModel>>();
        mapper.Setup(x => x.MapAsync(pagedResponse.Data, It.IsAny<CancellationToken>()))
            .ReturnsAsync([mappedViewModel]);

        var maxPageSize = new Mock<IMaxPageSize<Guid, ControllerTestEntity, ControllerTestVersion>>();
        maxPageSize.Setup(x => x.MaxPageSize).Returns(100);

        var controller = new TestChangeTrackingReadController(
            keyParser.Object,
            Options.Create(new ApiBehaviorOptions()),
            readService.Object,
            maxPageSize.Object,
            mapper.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetChangesAsync(entityId.ToString(), new ModifiedEntitySearchRequest { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<EntityPagedResponse<ControllerCustomViewModel>>().Subject;
        response.Data.Should().ContainSingle();
        response.Data.Single().RealActorEmail.Should().Be("actor@test.com");
    }
}
