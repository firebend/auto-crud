using System;
using System.Linq;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Ef;

/// <summary>
/// EF Core 10 refuses to generate SQL for any expression that has no type mapping, so every LIKE pattern the
/// JsonContainsAny translator emits (parameter or constant) must carry a string type mapping.
/// </summary>
[TestFixture]
public class JsonContainsAnyTests
{
    private static TestContext CreateContext()
    {
        var opt = new DbContextOptionsBuilder<TestContext>()
            .UseSqlServer()
            .AddFirebendFunctions()
            .Options;

        return new TestContext(opt);
    }

    [Test]
    public void JsonContainsAny_With_A_Parameter_Should_Translate_To_Like()
    {
        using var ctx = CreateContext();
        var search = "%widget%";

        var queryString = ctx.TestEntities
            .Where(x => EF.Functions.JsonContainsAny(x.Nested, search))
            .ToQueryString();

        // EF Core 9 names the parameter @__search_N; EF Core 10 names it @search.
        queryString.Should().MatchRegex(@"LIKE @\w*search\w* ESCAPE N'\\'");
        queryString.Should().MatchRegex(@"DECLARE @\w*search\w* nvarchar");
    }

    [Test]
    public void JsonContainsAny_With_A_Constant_Should_Wrap_It_In_Wildcards()
    {
        using var ctx = CreateContext();

        var queryString = ctx.TestEntities
            .Where(x => EF.Functions.JsonContainsAny(x.Nested, "widget"))
            .ToQueryString();

        queryString.Should().Contain("LIKE N'%widget%'");
    }

    [Test]
    public void JsonContainsAny_With_A_Constant_Containing_Like_Wildcards_Should_Escape_Them()
    {
        using var ctx = CreateContext();

        var queryString = ctx.TestEntities
            .Where(x => EF.Functions.JsonContainsAny(x.Nested, "50%_off"))
            .ToQueryString();

        queryString.Should().Contain("LIKE N'%50\\%\\_off%' ESCAPE N'\\'");
    }

    [Test]
    public void JsonContainsAny_With_An_Empty_Constant_Should_Not_Filter()
    {
        using var ctx = CreateContext();

        var queryString = ctx.TestEntities
            .Where(x => EF.Functions.JsonContainsAny(x.Nested, string.Empty))
            .ToQueryString();

        queryString.Should().NotContain("LIKE");
    }

    [Test]
    public void JsonContainsAny_On_A_Non_Column_Should_Not_Translate()
    {
        using var ctx = CreateContext();

        var act = () => ctx.TestEntities
            .Where(x => EF.Functions.JsonContainsAny(x.Name + "x", "widget"))
            .ToQueryString();

        act.Should().Throw<InvalidOperationException>().WithMessage("*could not be translated*");
    }
}
