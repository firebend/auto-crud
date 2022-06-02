using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.IntegrationTests.Models;
using Firebend.AutoCrud.Web.Sample.Models;
using FluentAssertions;
using Flurl.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class CustomFieldsApiTests
{
    private GetPersonViewModel _person;
    private const string PersonUrl = $"{TestConstants.BaseUrl}/v1/ef-person";

    [TestInitialize]
    public async Task TestSetup()
    {
        await TestRunner.Authenticate();
        _person = await CreatePersonAsync();
    }

    private async Task<GetPersonViewModel> CreatePersonAsync()
    {
        var faked = PersonFaker.Faker.Generate();
        var response = await PersonUrl.WithAuth()
            .PostJsonAsync(faked);

        var responseModel = await response.GetJsonAsync<GetPersonViewModel>();
        return responseModel;
    }

    private async Task<IFlurlResponse> CreateCustomFieldAsync(Guid personId, CustomFieldViewModel customField) =>
        await $"{PersonUrl}/{personId}/custom-fields".WithAuth().AllowHttpStatus(HttpStatusCode.BadRequest)
            .PostJsonAsync(customField);

    [TestMethod]
    [DataRow(0, HttpStatusCode.BadRequest)]
    [DataRow(1, HttpStatusCode.OK)]
    [DataRow(1000, HttpStatusCode.OK)]
    [DataRow(1001, HttpStatusCode.BadRequest)]
    public async Task CustomFieldsValueMinMaxCharacters(int valueLength, HttpStatusCode expectedStatusCode)
    {
        var customField = new CustomFieldViewModel {Key = "TestLength", Value = new Faker().Random.String(valueLength)};

        customField.Value.Length.Should().Be(valueLength);

        var customFieldResponse = await CreateCustomFieldAsync(_person.Id, customField);
        customFieldResponse.StatusCode.Should().Be((int) expectedStatusCode);
    }
}
