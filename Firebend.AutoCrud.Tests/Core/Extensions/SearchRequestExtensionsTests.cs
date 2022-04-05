using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.Searching;
using FluentAssertions;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core.Extensions;

[TestFixture]
public class SearchRequestExtensionsTests
{
    [TestCase(null)]
    [TestCase(-1)]
    [TestCase(0)]
    public void ValidateSearchRequest_Error_When_PageNumber(int? pageNumber)
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageNumber = pageNumber };

        //act
        var searchResult = searchRequest.ValidateSearchRequest(10);

        //assert
        searchResult.WasSuccessful.Should().BeFalse();
        searchResult.Errors[0].PropertyPath.Should().Be("PageNumber");
        searchResult.Errors[0].Error.Should().Be("Page number must be greater then 0.");
    }

    [TestCase(null)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(11)]
    public void ValidateSearchRequest_Error_When_PageSize_Is_Less_Then_Or_Equal_0(int? pageSize)
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageNumber = 1, PageSize = pageSize };

        //act
        var searchResult = searchRequest.ValidateSearchRequest(10);

        //assert
        searchResult.WasSuccessful.Should().BeFalse();
        searchResult.Errors[0].PropertyPath.Should().Be("PageSize");
        searchResult.Errors[0].Error.Should().Be("Page size must be between 1 and 10.");
    }

    [TestCase]
    public void ValidateSearchRequest_Error_When_CreateStartDate_Is_After_ModifiedStartDate()
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageNumber = 1, PageSize = 3 };
        searchRequest.CreatedStartDate = DateTimeOffset.Parse("2022-01-05T12:00:00-05:00");
        searchRequest.ModifiedStartDate = DateTimeOffset.Parse("1991-01-01T12:00:00-05:00");

        //act
        var searchResult = searchRequest.ValidateSearchRequest(10);

        //assert
        searchResult.WasSuccessful.Should().BeFalse();
        searchResult.Errors[0].PropertyPath.Should().Be("CreatedStartDate");
        searchResult.Errors[0].Error.Should().Be("Created date cannot be after modified date.");
    }

    [TestCase]
    public void ValidateSearchRequest_Error_When_CreatedStartDate_Is_After_CreatedEndDate()
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageNumber = 1, PageSize = 3 };
        searchRequest.CreatedStartDate = DateTimeOffset.Parse("2022-01-05T12:00:00-05:00");
        searchRequest.CreatedEndDate = DateTimeOffset.Parse("1991-01-01T12:00:00-05:00");

        //act
        var searchResult = searchRequest.ValidateSearchRequest(10);

        //assert
        searchResult.WasSuccessful.Should().BeFalse();
        searchResult.Errors[0].PropertyPath.Should().Be("CreatedStartDate");
        searchResult.Errors[0].Error.Should().Be("Created start date must be before end date.");
    }

    [TestCase]
    public void ValidateSearchRequest_Error_When_ModifiedStartDate_Is_After_ModifiedEndDate()
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageNumber = 1, PageSize = 3 };
        searchRequest.ModifiedStartDate = DateTimeOffset.Parse("2022-01-05T12:00:00-05:00");
        searchRequest.ModifiedEndDate = DateTimeOffset.Parse("1991-01-01T12:00:00-05:00");

        //act
        var searchResult = searchRequest.ValidateSearchRequest(10);

        //assert
        searchResult.WasSuccessful.Should().BeFalse();
        searchResult.Errors[0].PropertyPath.Should().Be("ModifiedStartDate");
        searchResult.Errors[0].Error.Should().Be("Modified start date must be before end date.");
    }

    [TestCase(1, 1, 6)]
    [TestCase(3, 6, 6)]
    [TestCase(6, 3, 6)]
    public void ValidateSearchRequest_Succeed(int? pageSize, int? pageNumber, int maxPageSize)
    {
        //arrange
        var searchRequest = new ActiveModifiedEntitySearchRequest { PageSize = pageSize, PageNumber = pageNumber };

        //act
        var searchResult = searchRequest.ValidateSearchRequest(maxPageSize);

        //assert
        searchResult.WasSuccessful.Should().BeTrue();
    }
}
