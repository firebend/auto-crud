using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.ObjectMapping;
using Firebend.AutoCrud.Io.Models;
using FluentAssertions;
using NUnit.Framework;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Firebend.AutoCrud.Tests.Core;

[TestFixture]
public class MapperTests
{
    private class SourceModelA
    {
        public int IntValue { get; set; }
        public int? NullableIntValue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset DtoValue { get; set; }
        public DateTimeOffset? NullableDtoValue { get; set; }
        public Guid GuidValue { get; set; }
        public Guid? NullableGuidValue { get; set; }
        public EntityFileType EnumValue { get; set; }
        public EntityFileType? NullableEnumValue { get; set; }
    }

    private class TargetModelA
    {
        public int IntValue { get; set; }
        public int? NullableIntValue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset DtoValue { get; set; }
        public DateTimeOffset? NullableDtoValue { get; set; }
        public Guid GuidValue { get; set; }
        public Guid? NullableGuidValue { get; set; }
        public EntityFileType EnumValue { get; set; }
        public EntityFileType? NullableEnumValue { get; set; }
    }

    [TestCase]
    public void Mapper_Should_Copy_Value_Types_Of_Same_Type()
    {
        // given
        var source = new SourceModelA
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet
        };
        var target = new TargetModelA();

        // when
        new ObjectMapper().Copy(source, target);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().Be(source.NullableEnumValue);
    }

    private class SourceModelB
    {
        public int IntValue { get; set; }
        public DateTimeOffset DtoValue { get; set; }
        public Guid GuidValue { get; set; }
        public EntityFileType EnumValue { get; set; }
    }

    private class TargetModelB
    {
        public int? IntValue { get; set; }
        public DateTimeOffset? DtoValue { get; set; }
        public Guid? GuidValue { get; set; }
        public EntityFileType? EnumValue { get; set; }
    }

    [TestCase]
    public void Mapper_Should_Copy_Value_Types_To_Nullable_Value_Types()
    {
        // given
        var source = new SourceModelB
        {
            IntValue = 1,
            DtoValue = DateTimeOffset.Now,
            GuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
        };
        var target = new TargetModelB();

        // when
        new ObjectMapper().Copy(source, target);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
    }

    private class SourceModelC : TargetModelB;

    private class TargetModelC : SourceModelB;

    [TestCase]
    public void Mapper_Should_Not_Copy_Nullable_Value_Types_To_Value_Types()
    {
        // given
        var source = new SourceModelC
        {
            IntValue = 1,
            DtoValue = DateTimeOffset.Now,
            GuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
        };
        var target = new TargetModelC();

        // when
        new ObjectMapper().Copy(source, target);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(default);
        target.DtoValue.Should().Be(default);
        target.GuidValue.Should().Be(default(Guid));
        target.EnumValue.Should().Be(default);
    }

    [TestCase]
    public void Mapper_Should_Not_Copy_Ignored_Properties()
    {
        // given
        var source = new SourceModelA
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet
        };
        var target = new TargetModelA();

        // when
        new ObjectMapper().Copy(source, target,
            new[] { nameof(SourceModelA.IntValue), nameof(SourceModelA.NullableEnumValue) });

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(default);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().BeNull();
    }

    [TestCase]
    public void Mapper_Should_Only_Copy_Included_Properties()
    {
        // given
        var source = new SourceModelA
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet
        };
        var target = new TargetModelA();

        // when
        new ObjectMapper().Copy(source, target,
            propertiesToInclude: new[] { nameof(SourceModelA.IntValue), nameof(SourceModelA.NullableEnumValue) });

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.NullableIntValue.Should().BeNull();
        target.StringValue.Should().Be(default);
        target.DtoValue.Should().Be(default);
        target.NullableDtoValue.Should().BeNull();
        target.GuidValue.Should().Be(default(Guid));
        target.NullableGuidValue.Should().BeNull();
        target.EnumValue.Should().Be(default);
        target.NullableEnumValue.Should().Be(source.NullableEnumValue);
    }

    [TestCase]
    public void Mapper_Should_Not_Copy_Ignored_Properties_After_Full_Copy()
    {
        // given
        var source = new SourceModelA
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet
        };
        var target = new TargetModelA();
        var dummy = new TargetModelA();

        // when
        var objectMapper = new ObjectMapper();
        objectMapper.Copy(source, dummy);
        objectMapper.Copy(source, target,
            new[] { nameof(SourceModelA.IntValue), nameof(SourceModelA.NullableEnumValue) });

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(default);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().BeNull();
    }

    private class NestedModel
    {
        public int IntValue { get; set; }
    }

    private class SourceModelD : SourceModelA
    {
        public NestedModel NestedModel { get; set; }
        public List<NestedModel> ListOfNestedModels { get; set; }
    }

    private class TargetModelD : TargetModelA
    {
        public NestedModel NestedModel { get; set; }
        // ReSharper disable once CollectionNeverUpdated.Local
        public List<NestedModel> ListOfNestedModels { get; set; }
    }

    [TestCase]
    public void Mapper_Should_Only_Copy_Value_Types_And_Strings_When_Include_Objects_False()
    {
        // given
        var source = new SourceModelD
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet,
            NestedModel = new NestedModel { IntValue = 3 },
            ListOfNestedModels = new List<NestedModel> { new NestedModel { IntValue = 4 } }
        };
        var target = new TargetModelD();

        // when
        new ObjectMapper().Copy(source, target, includeObjects: false);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().Be(source.NullableEnumValue);

        target.NestedModel.Should().BeNull();
        target.ListOfNestedModels.Should().BeNull();
    }

    [TestCase]
    public void Mapper_Should_Copy_Objects_And_Lists_Of_Same_Type_If_Include_Objects_True()
    {
        // given
        var source = new SourceModelD
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet,
            NestedModel = new NestedModel { IntValue = 3 },
            ListOfNestedModels = new List<NestedModel> { new NestedModel { IntValue = 4 } }
        };
        var target = new TargetModelD();

        // when
        new ObjectMapper().Copy(source, target, includeObjects: true);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().Be(source.NullableEnumValue);

        target.NestedModel.Should().Be(source.NestedModel);
        target.NestedModel.IntValue.Should().Be(source.NestedModel.IntValue);
        target.ListOfNestedModels.Should().BeSameAs(source.ListOfNestedModels);
        target.ListOfNestedModels.First().IntValue.Should().Be(source.ListOfNestedModels.First().IntValue);
    }

    private class NestedModelB
    {
        // ReSharper disable once UnusedMember.Local
        public int IntValue { get; set; }
    }

    private class TargetModelE : TargetModelA
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public NestedModelB NestedModel { get; set; }
        // ReSharper disable once CollectionNeverUpdated.Local
        public List<NestedModelB> ListOfNestedModels { get; set; }
    }

    [TestCase]
    public void Mapper_Should_Not_Copy_Objects_And_Lists_Of_Different_Types()
    {
        // given
        var source = new SourceModelD
        {
            IntValue = 1,
            NullableIntValue = 2,
            StringValue = "test",
            DtoValue = DateTimeOffset.Now,
            NullableDtoValue = DateTimeOffset.Now.AddHours(1),
            GuidValue = Guid.NewGuid(),
            NullableGuidValue = Guid.NewGuid(),
            EnumValue = EntityFileType.Csv,
            NullableEnumValue = EntityFileType.Spreadsheet
        };
        var target = new TargetModelE();

        // when
        new ObjectMapper().Copy(source, target, includeObjects: true);

        // then
        target.Should().NotBeNull();
        target.IntValue.Should().Be(source.IntValue);
        target.NullableIntValue.Should().Be(source.NullableIntValue);
        target.StringValue.Should().Be(source.StringValue);
        target.DtoValue.Should().Be(source.DtoValue);
        target.NullableDtoValue.Should().Be(source.NullableDtoValue);
        target.GuidValue.Should().Be(source.GuidValue);
        target.NullableGuidValue.Should().Be(source.NullableGuidValue);
        target.EnumValue.Should().Be(source.EnumValue);
        target.NullableEnumValue.Should().Be(source.NullableEnumValue);

        target.NestedModel.Should().BeNull();
        target.ListOfNestedModels.Should().BeNull();
    }
}
