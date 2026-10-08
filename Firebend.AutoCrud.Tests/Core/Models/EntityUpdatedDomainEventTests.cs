using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core.Models;

[TestFixture]
public class EntityUpdatedDomainEventTests
{
    public class PatchTestEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    private static IEnumerable<TestCaseData> SerializerSettings()
    {
        yield return new TestCaseData(new JsonSerializerSettings()).SetArgDisplayNames("Default");

        // Mirrors MassTransit.Newtonsoft's defaults (camelCase, ignore nulls/defaults) plus the
        // TypeNameHandling/ReferenceLoopHandling overrides the web sample applies to the bus.
        yield return new TestCaseData(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
        }).SetArgDisplayNames("MassTransit");
    }

    private static EntityUpdatedDomainEvent<PatchTestEntity> CreateEvent() => new()
    {
        Previous = new PatchTestEntity { Name = "before", Age = 30 },
        Operations =
        [
            new Operation<PatchTestEntity>("replace", "/name", null, "after"),
            new Operation<PatchTestEntity>("replace", "/age", null, 31),
        ],
    };

    [TestCaseSource(nameof(SerializerSettings))]
    public void Round_Trip_Should_Preserve_Previous_And_Operations_And_Rebuild_Patch_And_Modified(JsonSerializerSettings settings)
    {
        var original = CreateEvent();

        var json = JsonConvert.SerializeObject(original, settings);
        var deserialized = JsonConvert.DeserializeObject<EntityUpdatedDomainEvent<PatchTestEntity>>(json, settings);

        var properties = JObject.Parse(json).Properties();
        properties.Should().NotContain(p => p.Name.Equals("patch", System.StringComparison.OrdinalIgnoreCase));
        properties.Should().NotContain(p => p.Name.Equals("modified", System.StringComparison.OrdinalIgnoreCase));

        deserialized.Should().NotBeNull();
        deserialized.MessageId.Should().Be(original.MessageId);
        deserialized.Previous.Should().BeEquivalentTo(original.Previous);
        deserialized.Operations.Should().HaveCount(2);
        deserialized.Operations[0].op.Should().Be("replace");
        deserialized.Operations[0].path.Should().Be("/name");
        deserialized.Operations[1].path.Should().Be("/age");

        deserialized.Patch.Should().NotBeNull();
        deserialized.Patch.Operations.Should().HaveCount(2);

        deserialized.Modified.Should().NotBeNull();
        deserialized.Modified.Name.Should().Be("after");
        deserialized.Modified.Age.Should().Be(31);
        deserialized.Modified.Should().NotBeSameAs(deserialized.Previous);
        deserialized.Previous.Name.Should().Be("before");
        deserialized.Previous.Age.Should().Be(30);
    }

    [Test]
    public void Setting_Operations_Should_Reset_Cached_Patch_And_Modified()
    {
        var domainEvent = CreateEvent();

        var firstPatch = domainEvent.Patch;
        domainEvent.Modified.Name.Should().Be("after");

        domainEvent.Operations = [new Operation<PatchTestEntity>("replace", "/name", null, "changed again")];

        domainEvent.Patch.Should().NotBeSameAs(firstPatch);
        domainEvent.Patch.Operations.Should().ContainSingle();
        domainEvent.Modified.Name.Should().Be("changed again");
        domainEvent.Modified.Age.Should().Be(30);
    }

    [Test]
    public void Patch_And_Modified_Should_Be_Null_Without_Operations()
    {
        var domainEvent = new EntityUpdatedDomainEvent<PatchTestEntity>
        {
            Previous = new PatchTestEntity { Name = "before" },
            Operations = [],
        };

        domainEvent.Patch.Should().BeNull();
        domainEvent.Modified.Should().BeNull();

        domainEvent.Operations = null;

        domainEvent.Patch.Should().BeNull();
        domainEvent.Modified.Should().BeNull();
    }

    [Test]
    public void Modified_Should_Be_Null_Without_Previous()
    {
        var domainEvent = CreateEvent();
        domainEvent.Previous = null;

        domainEvent.Patch.Should().NotBeNull();
        domainEvent.Modified.Should().BeNull();
    }
}
