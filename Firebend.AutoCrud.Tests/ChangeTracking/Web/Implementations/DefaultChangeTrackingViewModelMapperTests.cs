using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.JsonPatch.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.Web.Implementations;

public class MapperTestVersion : IAutoCrudApiVersion
{
    public int Version => 1;
    public string Name => "Version 1";
}

public class MapperTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class MapperTestViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class MapperCustomRow : ChangeTrackingEntity<Guid, MapperTestEntity>
{
    public string RealActorEmail { get; set; }
}

public class MapperCustomViewModel : ChangeTrackingModel<Guid, MapperTestViewModel>
{
    public string RealActorEmail { get; set; }
}

[TestFixture]
public class DefaultChangeTrackingViewModelMapperTests
{
    [Test]
    public async Task MapAsync_CopiesCustomColumnByNameOntoViewModel()
    {
        var entity = new MapperTestEntity { Id = Guid.NewGuid(), Name = "Ada" };
        var viewModel = new MapperTestViewModel { Id = entity.Id, Name = entity.Name };

        var readViewModelMapper = new Mock<IReadViewModelMapper<Guid, MapperTestEntity, MapperTestVersion, MapperTestViewModel>>();
        readViewModelMapper.Setup(x => x.ToAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(viewModel);

        var patchGenerator = new Mock<IJsonPatchGenerator>();

        var sut = new DefaultChangeTrackingViewModelMapper<Guid, MapperTestEntity, MapperTestVersion, MapperTestViewModel, MapperCustomRow, MapperCustomViewModel>(
            readViewModelMapper.Object, patchGenerator.Object);

        var row = new MapperCustomRow
        {
            Id = Guid.NewGuid(),
            EntityId = entity.Id,
            Action = "Added",
            RealActorEmail = "actor@test.com",
            Entity = entity
        };

        var result = await sut.MapAsync([row], CancellationToken.None);

        result.Should().ContainSingle();
        result[0].RealActorEmail.Should().Be("actor@test.com");
        result[0].Entity.Should().Be(viewModel);
    }
}
