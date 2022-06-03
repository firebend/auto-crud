using System;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.CustomFields.Web.Models;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.IntegrationTests.Models;
using Firebend.AutoCrud.Web.Sample.Models;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
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

    private async Task<IFlurlResponse> PostCustomFieldAsync(Guid personId, CustomFieldViewModelCreate customField) =>
        await $"{PersonUrl}/{personId}/custom-fields".WithAuth().AllowHttpStatus(HttpStatusCode.BadRequest)
            .PostJsonAsync(customField);

    private async Task<IFlurlResponse> PutCustomFieldAsync(Guid personId, Guid customFieldId,
        CustomFieldViewModelCreate customField) =>
        await $"{PersonUrl}/{personId}/custom-fields/{customFieldId}".WithAuth()
            .AllowHttpStatus(HttpStatusCode.BadRequest)
            .PutJsonAsync(customField);

    private async Task<IFlurlResponse> PatchCustomFieldAsync(Guid personId, Guid customFieldId,
        JsonPatchDocument customField) =>
        await $"{PersonUrl}/{personId}/custom-fields/{customFieldId}".WithAuth()
            .AllowHttpStatus(HttpStatusCode.BadRequest)
            .PatchJsonAsync(customField);

    [TestMethod]
    [DataRow(0, HttpStatusCode.BadRequest)]
    [DataRow(1, HttpStatusCode.OK)]
    [DataRow(1000, HttpStatusCode.OK)]
    [DataRow(1001, HttpStatusCode.BadRequest)]
    public async Task CustomFieldsValueMinMaxCharacters(int valueLength, HttpStatusCode expectedStatusCode)
    {
        var customField =
            new CustomFieldViewModelCreate { Key = "TestLength", Value = new Faker().Random.String2(valueLength) };

        customField.Value.Length.Should().Be(valueLength);

        var customFieldResponse = await PostCustomFieldAsync(_person.Id, customField);
        customFieldResponse.StatusCode.Should().Be((int)expectedStatusCode);
    }

    [TestMethod]
    public async Task CustomFieldsPutValidation()
    {
        var faker = new Faker();
        var customFieldCreate = new CustomFieldViewModelCreate { Key = "Valid", Value = faker.Random.String2(100) };
        var customFieldRequest =
            await PostCustomFieldAsync(_person.Id, customFieldCreate);
        var customField = await customFieldRequest.GetJsonAsync<CustomFieldViewModelRead>();

        customField.Should().NotBeNull();
        customField.Key.Should().Be(customFieldCreate.Key);
        customField.Value.Should().Be(customFieldCreate.Value);

        var validPutBody = new CustomFieldViewModelCreate { Key = customField.Key, Value = faker.Random.String2(50) };
        var validPutResponse = await PutCustomFieldAsync(_person.Id, customField.Id, validPutBody);
        validPutResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var invalidPutBody = new CustomFieldViewModelCreate { Key = customField.Key, Value = faker.Random.String2(1001) };
        var invalidPutResponse = await PutCustomFieldAsync(_person.Id, customField.Id, invalidPutBody);
        invalidPutResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task CustomFieldsPatchValidation()
    {
        var faker = new Faker();
        var customFieldCreate = new CustomFieldViewModelCreate { Key = "Valid", Value = faker.Random.String2(100) };
        var customFieldRequest =
            await PostCustomFieldAsync(_person.Id, customFieldCreate);
        var customField = await customFieldRequest.GetJsonAsync<CustomFieldViewModelRead>();

        customField.Should().NotBeNull();
        customField.Key.Should().Be(customFieldCreate.Key);
        customField.Value.Should().Be(customFieldCreate.Value);

        var validPatchDocument =
            PatchFaker.MakeReplacePatch<CustomFieldViewModelCreate, string>(x => x.Value, faker.Random.String2(50));
        var validPatchResponse = await PatchCustomFieldAsync(_person.Id, customField.Id, validPatchDocument);
        validPatchResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var invalidPatchBody =
            PatchFaker.MakeReplacePatch<CustomFieldViewModelCreate, string>(x => x.Value, faker.Random.String2(1001));
        var invalidPatchResponse = await PatchCustomFieldAsync(_person.Id, customField.Id, invalidPatchBody);
        invalidPatchResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task CustomFieldsPerEntityLimit()
    {
        var person = await CreatePersonAsync();
        var customFields = CustomFieldFaker.Faker.Generate(10);
        foreach (var customField in customFields)
        {
            var added = await PostCustomFieldAsync(person.Id, customField);
            added.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        var oneTooMany = CustomFieldFaker.Faker.Generate();
        var oneTooManyResponse = await PostCustomFieldAsync(person.Id, oneTooMany);

        oneTooManyResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        (await oneTooManyResponse.GetStringAsync()).Should().Contain("Only 10 custom fields allowed per EfPerson");
    }
}
