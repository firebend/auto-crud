using System;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using FluentAssertions;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core;

[TestFixture]
public class DefaultEntityQueryOrderByHandlerTests
{
    public class RefEntity
    {
        public string Color { get; set; }
    }

    public class TestEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Date { get; set; }
        public int Integer { get; set; }
        public decimal Decimal { get; set; }
        public RefEntity Ref { get; set; }
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Asc()
    {
        const string orderBy = "name:asc";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("0");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Desc_Date()
    {
        const string orderBy = "date:desc";
        var result = CreateTestEntities(orderBy, false, (entity, s) =>
        {
            entity.Date = DateTimeOffset.Now.AddDays(s);
            entity.Name = s.ToString();
        });
        result.FirstOrDefault()!.Name.Should().Be("4");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Desc_Int()
    {
        const string orderBy = "integer:desc";
        var result = CreateTestEntities(orderBy, false, (entity, s) => entity.Integer = s);
        result.FirstOrDefault()!.Integer.Should().Be(4);
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Asc_Int()
    {
        const string orderBy = "integer:asc";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Integer = s);
        result.FirstOrDefault()!.Integer.Should().Be(0);
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Desc_Decimal()
    {
        const string orderBy = "decimal:desc";
        var result = CreateTestEntities(orderBy, false, (entity, s) => entity.Decimal = s);
        result.FirstOrDefault()!.Decimal.Should().Be(4);
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Asc_When_Order_Not_Specified()
    {
        const string orderBy = "name";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("0");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_Desc()
    {
        const string orderBy = "ref.color:desc";
        var result = CreateTestEntities(orderBy, false, (entity, s) => entity.Ref.Color = s.ToString());
        result.FirstOrDefault()!.Ref.Color.Should().Be("0");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_By_Nested_Object_Asc()
    {
        const string orderBy = "name:asc";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("0");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Order_By_Nested_Object_Desc()
    {
        const string orderBy = "name:desc";
        var result = CreateTestEntities(orderBy, false, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("4");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Handle_Invalid_Field_Name_Asc()
    {
        const string orderBy = "fake:asc";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("0");
    }

    [TestCase]
    public void Default_Entity_Query_By_Handler_Should_Handle_Invalid_Field_Name_Desc()
    {
        const string orderBy = "fake:desc";
        var result = CreateTestEntities(orderBy, true, (entity, s) => entity.Name = s.ToString());
        result.FirstOrDefault()!.Name.Should().Be("0");
    }

    private static IQueryable<TestEntity> CreateTestEntities(string orderBy, bool assignInReverse, Action<TestEntity, int> assigner)
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        var list = fixture.CreateMany<TestEntity>(5).ToArray();

        if (assignInReverse is false)
        {
            for (var index = 0; index < list.Length; index++)
            {
                assigner(list[index], index);
            }
        }
        else
        {
            for (var index = list.Length - 1; index >= 0; index--)
            {
                assigner(list[index], index);
            }
        }

        var sut = new DefaultEntityQueryOrderByHandler<Guid, TestEntity>();
        var result = sut.OrderBy(list.AsQueryable(), new[]
        {
            orderBy
        }.ToOrderByGroups<TestEntity>());
        return result;
    }
}
