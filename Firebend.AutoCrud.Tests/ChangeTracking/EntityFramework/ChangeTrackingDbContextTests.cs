using System;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.EntityFramework;

public class ChangeTrackingDbContextTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
}

public class CustomChangeTrackingRow : ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>
{
    public string RealActorEmail { get; set; }
}

public class FakeChangeTrackingTableNameProvider : IChangeTrackingTableNameProvider<Guid, ChangeTrackingDbContextTestEntity>
{
    public TableNameResult GetTableName() => new("ChangeTrackingDbContextTestEntity_Changes", "dbo");
}

[TestFixture]
public class ChangeTrackingDbContextTests
{
    private static ChangeTrackingDbContext<Guid, ChangeTrackingDbContextTestEntity, TRow> BuildContext<TRow>()
        where TRow : ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>
    {
        var options = new DbContextOptionsBuilder<ChangeTrackingDbContext<Guid, ChangeTrackingDbContextTestEntity, TRow>>()
            .UseSqlServer("Data Source=.;Initial Catalog=Ignored;Integrated Security=True;")
            .Options;

        return new ChangeTrackingDbContext<Guid, ChangeTrackingDbContextTestEntity, TRow>(
            options, new FakeChangeTrackingTableNameProvider());
    }

    [Test]
    public void DefaultRowType_MapsSingleEntityType_WithNoDiscriminator()
    {
        using var context = BuildContext<ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>>();

        var entityType = context.Model.FindEntityType(typeof(ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>));

        entityType.Should().NotBeNull();
        entityType!.FindDiscriminatorProperty().Should().BeNull();
        entityType.GetTableName().Should().Be("ChangeTrackingDbContextTestEntity_Changes");
    }

    [Test]
    public void CustomRowType_MapsOwnScalarPropertiesAutomatically_WithNoDiscriminator()
    {
        using var context = BuildContext<CustomChangeTrackingRow>();

        var entityType = context.Model.FindEntityType(typeof(CustomChangeTrackingRow));

        entityType.Should().NotBeNull();
        entityType!.FindDiscriminatorProperty().Should().BeNull();
        entityType.FindProperty(nameof(CustomChangeTrackingRow.RealActorEmail)).Should().NotBeNull();

        // The base type is NOT separately queryable once only the derived type is mapped -
        // this is why the read path (Task 4) must stay pinned to the default row type.
        context.Model.FindEntityType(typeof(ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>)).Should().BeNull();
    }

    [Test]
    public void DefaultRowType_IndexesEntityIdThenModifiedDate_WithPinnedName()
    {
        using var context = BuildContext<ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>>();

        var entityType = context.Model.FindEntityType(typeof(ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>));

        var index = entityType!.GetIndexes().Should().ContainSingle().Subject;
        index.Properties.Select(p => p.Name).Should().Equal(
            nameof(ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>.EntityId),
            nameof(ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>.ModifiedDate));
        index.GetDatabaseName().Should().Be("IX_ChangeTrackingDbContextTestEntity_Changes_EntityId_ModifiedDate");
    }

    [Test]
    public void CustomRowType_KeepsTheIndex()
    {
        using var context = BuildContext<CustomChangeTrackingRow>();

        var entityType = context.Model.FindEntityType(typeof(CustomChangeTrackingRow));

        entityType!.GetIndexes().Select(i => i.GetDatabaseName())
            .Should().Equal("IX_ChangeTrackingDbContextTestEntity_Changes_EntityId_ModifiedDate");
    }

    [Test]
    public void CreateScript_CreatesTheIndexWithTheTable()
    {
        using var context = BuildContext<ChangeTrackingEntity<Guid, ChangeTrackingDbContextTestEntity>>();

        var script = context.Database.GenerateCreateScript();

        script.Should().Contain(
            "CREATE INDEX [IX_ChangeTrackingDbContextTestEntity_Changes_EntityId_ModifiedDate] " +
            "ON [dbo].[ChangeTrackingDbContextTestEntity_Changes] ([EntityId], [ModifiedDate]);");
    }
}
