using System;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Mongo;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.Mongo;

public class MongoExtensionsTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
}

public class MongoExtensionsCustomRow : ChangeTrackingEntity<Guid, MongoExtensionsTestEntity>, IAuditContextProperties
{
    public string RealActorEmail { get; set; }
    public void PopulateFrom(DomainEventContext context) => RealActorEmail = context?.UserEmail;
}

[TestFixture]
public class ExtensionsTests
{
    private static DomainEventsConfigurator<MongoDbEntityBuilder<Guid, MongoExtensionsTestEntity>, Guid, MongoExtensionsTestEntity> BuildConfigurator()
    {
        var builder = new MongoDbEntityBuilder<Guid, MongoExtensionsTestEntity>(new ServiceCollection());

        return new DomainEventsConfigurator<MongoDbEntityBuilder<Guid, MongoExtensionsTestEntity>, Guid, MongoExtensionsTestEntity>(builder);
    }

    [Test]
    public void WithMongoChangeTracking_DefaultOverload_RegistersReadService()
    {
        var configurator = BuildConfigurator();

        configurator.WithMongoChangeTracking(changeTracking => changeTracking.WithConnectionString("mongodb://localhost:27017"));

        configurator.Builder.Registrations.Should().ContainKey(typeof(IChangeTrackingReadService<Guid, MongoExtensionsTestEntity>));
        configurator.Builder.Registrations.Should().ContainKey(typeof(IChangeTrackingService<Guid, MongoExtensionsTestEntity>));
    }

    [Test]
    public void WithMongoChangeTracking_CustomRowTypeOverload_RegistersWritePath_ButNotReadService()
    {
        var configurator = BuildConfigurator();

        configurator.WithMongoChangeTracking<MongoDbEntityBuilder<Guid, MongoExtensionsTestEntity>, Guid, MongoExtensionsTestEntity, MongoExtensionsCustomRow>(
            changeTracking => changeTracking.WithConnectionString("mongodb://localhost:27017"));

        configurator.Builder.Registrations.Should().ContainKey(typeof(IChangeTrackingService<Guid, MongoExtensionsTestEntity>));
        configurator.Builder.Registrations.Should().NotContainKey(typeof(IChangeTrackingReadService<Guid, MongoExtensionsTestEntity>));
    }

    [Test]
    public void WithMongoChangeTracking_CustomRowTypeOverload_RegistersTier2ReadService()
    {
        var configurator = BuildConfigurator();

        configurator.WithMongoChangeTracking<MongoDbEntityBuilder<Guid, MongoExtensionsTestEntity>, Guid, MongoExtensionsTestEntity, MongoExtensionsCustomRow>(
            changeTracking => changeTracking.WithConnectionString("mongodb://localhost:27017"));

        configurator.Builder.Registrations.Should()
            .ContainKey(typeof(IChangeTrackingReadService<Guid, MongoExtensionsTestEntity, MongoExtensionsCustomRow>));
    }

    [Test]
    public void WithMongoChangeTracking_DefaultOverload_RegistersTier2ReadServiceToo()
    {
        var configurator = BuildConfigurator();

        configurator.WithMongoChangeTracking(changeTracking => changeTracking.WithConnectionString("mongodb://localhost:27017"));

        configurator.Builder.Registrations.Should()
            .ContainKey(typeof(IChangeTrackingReadService<Guid, MongoExtensionsTestEntity, ChangeTrackingEntity<Guid, MongoExtensionsTestEntity>>));
    }
}
