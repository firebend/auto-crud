using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core.Extensions;

[TestFixture]
public class JsonPatchExtensionTests
{

    private class PatchTestClass : IModifiedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }

    [TestCase]
    public void Is_Only_Modified_Entity_Patch_Should_Be_True_When_Only_Modified()
    {
        var patch = new JsonPatchDocument<PatchTestClass>();
        patch.Replace(x => x.ModifiedDate, DateTimeOffset.UtcNow);

        patch.IsOnlyModifiedEntityPatch().Should().BeTrue();
    }

    [TestCase]
    public void Is_Only_Modified_Entity_Patch_Should_Be_True_When_Only_Created()
    {
        var patch = new JsonPatchDocument<PatchTestClass>();
        patch.Replace(x => x.CreatedDate, DateTimeOffset.UtcNow);

        patch.IsOnlyModifiedEntityPatch().Should().BeTrue();
    }

    [TestCase]
    public void Is_Only_Modified_Entity_Patch_Should_Be_True_When_Created_And_Modified()
    {
        var patch = new JsonPatchDocument<PatchTestClass>();
        patch.Replace(x => x.CreatedDate, DateTimeOffset.UtcNow);
        patch.Replace(x => x.ModifiedDate, DateTimeOffset.UtcNow);

        patch.IsOnlyModifiedEntityPatch().Should().BeTrue();
    }

    [TestCase]
    public void Is_Only_Modified_Entity_Patch_Should_Be_False_When_Created_And_Modified_And_Other_Property()
    {
        var patch = new JsonPatchDocument<PatchTestClass>();
        patch.Replace(x => x.CreatedDate, DateTimeOffset.UtcNow);
        patch.Replace(x => x.ModifiedDate, DateTimeOffset.UtcNow);
        patch.Replace(x => x.Description, "Fake");

        patch.IsOnlyModifiedEntityPatch().Should().BeFalse();
    }

    [TestCase]
    public void Is_Only_Modified_Entity_Patch_Should_Be_False_When_Other_Property()
    {
        var patch = new JsonPatchDocument<PatchTestClass>();
        patch.Replace(x => x.Description, "Fake");

        patch.IsOnlyModifiedEntityPatch().Should().BeFalse();
    }
}
