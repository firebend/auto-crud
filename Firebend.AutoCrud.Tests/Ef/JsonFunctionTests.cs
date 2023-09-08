using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Ef;


[TestFixture]
public class JsonFunctionTests
{
    [Test]
    public async Task Json_Functions_Is_Json_Should_Work()
    {
        await using var ctx = TestDbContextFactory.Create();
        var query = ctx.TestEntities.Where(x => EfJsonFunctions.IsJson(nameof(TestEntity.Nested)) == 1);
        var queryString = query.ToQueryString();
        queryString.Should().NotBeNullOrWhiteSpace();
        queryString.Should().ContainEquivalentOf("ISJSON(Nested) = 1");
    }

    [Test]
    public async Task Json_Functions_Json_Path_Exists_Should_Work()
    {
        await using var ctx = TestDbContextFactory.Create();
        var query = ctx.TestEntities
            .Where(x => EfJsonFunctions.JsonPathExists(nameof(TestEntity.Nested), "$.stringField") == 1);
        var queryString = query.ToQueryString();
        queryString.Should().NotBeNullOrWhiteSpace();
        queryString.Should().ContainEquivalentOf("JSON_PATH_EXISTS(Nested, N'$.stringField') = 1");
    }

    [Test]
    public async Task Json_Functions_Json_Query_Should_Work()
    {
        await using var ctx = TestDbContextFactory.Create();
        var query = ctx.TestEntities
            .Where(x => EfJsonFunctions.JsonQuery(nameof(TestEntity.Nested), "$.guidList").Contains("420"));
        var queryString = query.ToQueryString();
        queryString.Should().NotBeNullOrWhiteSpace();
        queryString.Should().ContainEquivalentOf("JSON_QUERY(Nested, N'$.guidList') LIKE N'%420%");
    }

    [Test]
    public async Task Json_Functions_Json_Value_Should_Work()
    {
        await using var ctx = TestDbContextFactory.Create();
        var query = ctx.TestEntities
            .Where(x => EfJsonFunctions.JsonValue(nameof(TestEntity.Nested), "$.stringField").Contains("420"));
        var queryString = query.ToQueryString();
        queryString.Should().NotBeNullOrWhiteSpace();
        queryString.Should().ContainEquivalentOf("JSON_VALUE(Nested, N'$.stringField') LIKE N'%420%");
    }

    [Test]
    public async Task Json_Functions_Json_Array_Is_Empty_Should_Work()
    {
        await using var ctx = TestDbContextFactory.Create();
        var query = ctx.TestEntities
            .Where(x => EfJsonFunctions.JsonArrayIsEmpty(nameof(TestEntity.Nested), "$.guidList"));
        var queryString = query.ToQueryString();
        queryString.Should().NotBeNullOrWhiteSpace();
        queryString.Should().ContainEquivalentOf("JSON_QUERY(Nested, N'$.guidList') = N'[]'");
    }
}
