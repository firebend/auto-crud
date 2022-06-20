using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.CustomFields.Web.Models;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class MongoCustomFieldsApiTests
{
    private GetPersonViewModel _person;
    private (GetPersonViewModel person, List<CustomFieldViewModelCreate> customFields) _personWith10CustomFields;
    private const string PersonUrl = $"{TestConstants.BaseUrl}/v1/mongo-person";

    [TestInitialize]
    public async Task TestSetup()
    {
        await TestRunner.Authenticate();
        _person = await CreatePersonAsync();
        await CreatePersonWith10CustomFields();
    }

    private async Task CreatePersonWith10CustomFields()
    {
        if (_personWith10CustomFields.person != null)
        {
            return;
        }

        var person = await CreatePersonAsync();
        var customFields = CustomFieldFaker.Faker.Generate(10);
        foreach (var customField in customFields)
        {
            var added = await PostCustomFieldAsync(person.Id, customField);
            added.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        _personWith10CustomFields = (person, customFields);
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
    public async Task CustomFieldsPutMinMaxValidation()
    {
        var faker = new Faker();
        var customFieldCreate = new CustomFieldViewModelCreate { Key = "Valid", Value = faker.Random.String2(100) };
        var customFieldRequest =
            await PostCustomFieldAsync(_person.Id, customFieldCreate);
        var customField = await customFieldRequest.GetJsonAsync<CustomFieldViewModel>();

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
    public async Task CustomFieldsPatchMinMaxValidation()
    {
        var faker = new Faker();
        var customFieldCreate = new CustomFieldViewModelCreate { Key = "Valid", Value = faker.Random.String2(100) };
        var customFieldRequest =
            await PostCustomFieldAsync(_person.Id, customFieldCreate);
        var customField = await customFieldRequest.GetJsonAsync<CustomFieldViewModel>();

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
        var oneTooMany = CustomFieldFaker.Faker.Generate();
        var oneTooManyResponse = await PostCustomFieldAsync(_personWith10CustomFields.person.Id, oneTooMany);

        oneTooManyResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        (await oneTooManyResponse.GetStringAsync()).Should().Contain("Only 10 custom fields allowed per MongoTenantPerson");
    }

    [TestMethod]
    public async Task CustomFieldsPatchValidation()
    {
        var customField = CustomFieldFaker.Faker.Generate();
        var customFieldResponse = await PostCustomFieldAsync(_person.Id, customField);
        customFieldResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var customFieldResult = await customFieldResponse.GetJsonAsync<CustomFieldViewModel>();

        var patch = PatchFaker.MakeReplacePatch<CustomFieldViewModelCreate, string>(x =>
            x.Value, "All your base are belong to us!");
        var patchResponse = await PatchCustomFieldAsync(_person.Id, customFieldResult.Id, patch);
        patchResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var patchResult = await patchResponse.GetJsonAsync<CustomFieldViewModel>();
        patchResult.Value.Should()
            .Be("With the help of Federation government forces, CATS has taken all of your bases.");
    }

    [TestMethod]
    public async Task EfCustomFieldsGetAll()
    {
        var response = await $"{PersonUrl}/{_personWith10CustomFields.person.Id}/custom-fields".WithAuth()
            .GetJsonAsync<List<CustomFieldViewModel>>();

        response.Should().NotBeNull();
        response.Should().HaveCount(10);
    }

    [TestMethod]
    public async Task EfCustomFieldsGetAllWithFilter()
    {
        var (person, customFields) = _personWith10CustomFields;
        var responseValid = await $"{PersonUrl}/{person.Id}/custom-fields?key={customFields[0].Key}".WithAuth()
            .GetJsonAsync<List<CustomFieldViewModel>>();
        var responseInValid = await $"{PersonUrl}/{person.Id}/custom-fields?key=nope".WithAuth()
            .GetJsonAsync<List<CustomFieldViewModel>>();

        responseValid.Should().NotBeNull();
        responseValid.Should().HaveCount(customFields.Count(x => x.Key == customFields[0].Key));

        responseInValid.Should().NotBeNull();
        responseInValid.Should().BeEmpty();
    }

    [TestMethod]
    public async Task EfCustomFieldsGetById()
    {
        var customField = CustomFieldFaker.Faker.Generate();
        var added = await PostCustomFieldAsync(_person.Id, customField);
        var addedResponse = await added.GetJsonAsync<CustomFieldViewModel>();
        added.StatusCode.Should().Be((int)HttpStatusCode.OK);

        var response = await $"{PersonUrl}/{_person.Id}/custom-fields/{addedResponse.Id}".WithAuth()
            .GetJsonAsync<CustomFieldViewModel>();

        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(addedResponse);
    }

    [TestMethod]
    public async Task EfCustomFieldsExists()
    {
        var (person, customFields) = _personWith10CustomFields;

        var responseValid = await $"{PersonUrl}/{person.Id}/custom-fields/exists?key={customFields[0].Key}".WithAuth()
            .GetJsonAsync<bool>();

        var responseInValid = await $"{PersonUrl}/{person.Id}/custom-fields/exists?key=nope".WithAuth()
            .GetJsonAsync<bool>();

        responseValid.Should().BeTrue();

        responseInValid.Should().BeFalse();
    }

    [TestMethod]
    public async Task EfCustomFieldsFirst()
    {
        var (person, customFields) = _personWith10CustomFields;

        var responseValid = await $"{PersonUrl}/{person.Id}/custom-fields/first?key={customFields[0].Key}".WithAuth()
            .GetJsonAsync<CustomFieldViewModel>();

        var responseInValid = await $"{PersonUrl}/{person.Id}/custom-fields/first?key=nope".WithAuth()
            .GetJsonAsync<CustomFieldViewModel>();

        responseValid.Should().NotBeNull();
        responseValid.Should().BeEquivalentTo(customFields[0]);

        responseInValid.Should().BeNull();
    }
}
