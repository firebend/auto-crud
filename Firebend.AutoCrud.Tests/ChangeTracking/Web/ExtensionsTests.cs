using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations;
using Firebend.AutoCrud.ChangeTracking.Web.Interfaces;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.Tests.Web.Implementations.Swagger;
using Firebend.AutoCrud.Web;
using Firebend.AutoCrud.Web.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.Web;

public class WebExtensionsTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
}

public class WebExtensionsTestViewModel
{
    public Guid Id { get; set; }
}

public class WebExtensionsCustomRow : ChangeTrackingEntity<Guid, WebExtensionsTestEntity>
{
    public string RealActorEmail { get; set; }
}

public class WebExtensionsCustomViewModel : ChangeTrackingModel<Guid, WebExtensionsTestViewModel>
{
    public string RealActorEmail { get; set; }
}

public class WebExtensionsTestDbContext(DbContextOptions<WebExtensionsTestDbContext> options) : AbstractDbContext(options);

public class WebExtensionsFakeReadViewModelMapper : IReadViewModelMapper<Guid, WebExtensionsTestEntity, V1, WebExtensionsTestViewModel>
{
    public Task<WebExtensionsTestEntity> FromAsync(WebExtensionsTestViewModel model, CancellationToken cancellationToken)
        => Task.FromResult(new WebExtensionsTestEntity { Id = model.Id });

    public Task<System.Collections.Generic.IEnumerable<WebExtensionsTestEntity>> FromAsync(
        System.Collections.Generic.IEnumerable<WebExtensionsTestViewModel> model, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<WebExtensionsTestViewModel> ToAsync(WebExtensionsTestEntity entity, CancellationToken cancellationToken)
        => Task.FromResult(new WebExtensionsTestViewModel { Id = entity.Id });

    public Task<System.Collections.Generic.IEnumerable<WebExtensionsTestViewModel>> ToAsync(
        System.Collections.Generic.IEnumerable<WebExtensionsTestEntity> entity, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}

[TestFixture]
public class ExtensionsTests
{
    private static EntityFrameworkEntityBuilder<Guid, WebExtensionsTestEntity> BuildBuilder()
        => new(
            new ServiceCollection(),
            typeof(WebExtensionsTestDbContext),
            (_, options) => options.UseSqlServer("Data Source=.;Initial Catalog=Ignored;Integrated Security=True;"),
            false);

    [Test]
    public void ChangeTrackingControllerType_AfterHelperRefactor_StillReturnsTier1ControllerType()
    {
        var builder = BuildBuilder();
        Type resolvedType = null;

        builder.AddControllers<Guid, WebExtensionsTestEntity, V1>(configure =>
        {
            configure.WithReadViewModel<WebExtensionsTestViewModel, WebExtensionsFakeReadViewModelMapper>();
            resolvedType = configure.ChangeTrackingControllerType();
        });

        resolvedType.Should().Be(typeof(AbstractChangeTrackingReadController<,,,>)
            .MakeGenericType(typeof(Guid), typeof(WebExtensionsTestEntity), typeof(V1), typeof(WebExtensionsTestViewModel)));
    }

    [Test]
    public void WithChangeTrackingControllers_Tier2_RegistersDefaultMapperAndController()
    {
        var builder = BuildBuilder();

        builder.AddControllers<Guid, WebExtensionsTestEntity, V1>(configure =>
        {
            configure.WithReadViewModel<WebExtensionsTestViewModel, WebExtensionsFakeReadViewModelMapper>();
            configure.WithChangeTrackingControllers<EntityCrudBuilder<Guid, WebExtensionsTestEntity>, Guid, WebExtensionsTestEntity, V1, WebExtensionsCustomRow>();
        });

        var expectedDefaultViewModelType = typeof(ChangeTrackingModel<,>).MakeGenericType(typeof(Guid), typeof(WebExtensionsTestViewModel));

        var expectedMapperType = typeof(IChangeTrackingViewModelMapper<,,,,,>).MakeGenericType(
            typeof(Guid), typeof(WebExtensionsTestEntity), typeof(V1), typeof(WebExtensionsTestViewModel),
            typeof(WebExtensionsCustomRow), expectedDefaultViewModelType);

        var expectedControllerType = typeof(AbstractChangeTrackingReadController<,,,,,>).MakeGenericType(
            typeof(Guid), typeof(WebExtensionsTestEntity), typeof(V1), typeof(WebExtensionsTestViewModel),
            typeof(WebExtensionsCustomRow), expectedDefaultViewModelType);

        builder.Registrations.Should().ContainKey(expectedMapperType);
        builder.Registrations.Should().ContainKey(expectedControllerType);
    }

    [Test]
    public void WithChangeTrackingControllers_Tier3_RegistersCustomMapperAndController()
    {
        var builder = BuildBuilder();

        builder.AddControllers<Guid, WebExtensionsTestEntity, V1>(configure =>
        {
            configure.WithReadViewModel<WebExtensionsTestViewModel, WebExtensionsFakeReadViewModelMapper>();
            configure.WithChangeTrackingControllers<EntityCrudBuilder<Guid, WebExtensionsTestEntity>, Guid, WebExtensionsTestEntity, V1, WebExtensionsCustomRow, WebExtensionsCustomViewModel>();
        });

        var expectedMapperType = typeof(IChangeTrackingViewModelMapper<,,,,,>).MakeGenericType(
            typeof(Guid), typeof(WebExtensionsTestEntity), typeof(V1), typeof(WebExtensionsTestViewModel),
            typeof(WebExtensionsCustomRow), typeof(WebExtensionsCustomViewModel));

        var expectedControllerType = typeof(AbstractChangeTrackingReadController<,,,,,>).MakeGenericType(
            typeof(Guid), typeof(WebExtensionsTestEntity), typeof(V1), typeof(WebExtensionsTestViewModel),
            typeof(WebExtensionsCustomRow), typeof(WebExtensionsCustomViewModel));

        builder.Registrations.Should().ContainKey(expectedMapperType);
        builder.Registrations.Should().ContainKey(expectedControllerType);
    }
}
