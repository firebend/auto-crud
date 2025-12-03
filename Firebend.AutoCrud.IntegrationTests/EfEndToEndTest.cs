using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class EfEndToEndTest : BaseTest<
    Guid,
    PersonViewModelBase,
    PersonViewModelBase,
    GetPersonViewModel,
    PersonExport>
{
    protected override string Url => $"{TestConstants.BaseUrl}/v1/ef-person";

    [TestMethod]
    public async Task Ef_Api_Should_Work() => await EndToEndAsync(x => x.FirstName, false);

    [TestMethod]
    public async Task Ef_Delete_Validation_Should_Fail()
    {
        await TestRunner.Authenticate();
        var faked = await GenerateCreateRequestAsync();
        faked.FirstName = "Block";
        var created = await PostAsync(faked);

        var deleteResponse = await $"{Url}/{created.Id}".WithAuth().AllowHttpStatus(400).DeleteAsync();

        deleteResponse.Should().NotBeNull();
        deleteResponse.StatusCode.Should().Be(400);

        var deleteResponseModel = await deleteResponse.GetJsonAsync<ValidationProblemDetails>();
        deleteResponseModel.Should().NotBeNull();
        deleteResponseModel.Errors.Should().NotBeNull();
        deleteResponseModel.Errors.Should().ContainKey(nameof(PersonViewModelBase.FirstName));
        deleteResponseModel.Errors[nameof(PersonViewModelBase.FirstName)].Should()
            .Contain("Cannot delete a person with the first name 'Block'");
    }

    [TestMethod]
    public async Task Export_Should_Work()
    {
        await TestRunner.Authenticate();
        var nickName = Guid.NewGuid().ToString();

        const string personHeader = "FirstName,LastName,FullName,Id,CreatedDate,ModifiedDate";
        const string petHeader = "Id,EfPersonId,PetName,PetType,CreatedDate,ModifiedDate";
        const string customFieldHeader = "EntityId,Key,Value,Id,CreatedDate,ModifiedDate";

        var exportResult = await GetExportAsync(nickName);
        exportResult.Should().NotContain(nickName);

        var person1 = await CreatePersonAsync(nickName);
        exportResult = await GetExportAsync(nickName);
        exportResult.Should().Contain(person1.Id.ToString());
        Regex.Matches(exportResult, personHeader).Count.Should().Be(1);
        Regex.Matches(exportResult, $"\r\n\r\n\r\n{personHeader}").Count.Should().Be(0);
        Regex.Matches(exportResult, petHeader).Count.Should().Be(0);
        Regex.Matches(exportResult, customFieldHeader).Count.Should().Be(0);

        // get export check for two people and one with pet
        var person2 = await CreatePersonAsync(nickName);
        var pet1 = await CreatePetAsync(person2.Id);
        exportResult = await GetExportAsync(nickName);
        exportResult.Should().Contain(person1.Id.ToString());
        exportResult.Should().Contain(person2.Id.ToString());
        exportResult.Should().Contain(pet1.Id.ToString());
        Regex.Matches(exportResult, personHeader).Count.Should().Be(2);
        Regex.Matches(exportResult, $"\r\n\r\n\r\n{personHeader}").Count.Should().Be(1);
        Regex.Matches(exportResult, $"\r\n\r\n{petHeader}").Count.Should().Be(1);
        Regex.Matches(exportResult, customFieldHeader).Count.Should().Be(0);

        // get export check for three, one with two pets, and one with pet plus custom fields
        var pet3 = await CreatePetAsync(person2.Id);
        var person3 = await CreatePersonAsync(nickName);
        var pet2 = await CreatePetAsync(person3.Id);
        var customField = await CreatePetCustomFieldAsync(person3.Id, pet2.Id);
        exportResult = await GetExportAsync(nickName);
        exportResult.Should().Contain(person1.Id.ToString());
        exportResult.Should().Contain(person2.Id.ToString());
        exportResult.Should().Contain(person3.Id.ToString());
        exportResult.Should().Contain(pet1.Id.ToString());
        exportResult.Should().Contain(pet2.Id.ToString());
        exportResult.Should().Contain(pet3.Id.ToString());
        exportResult.Should().Contain(customField.Id.ToString());
        Regex.Matches(exportResult, personHeader).Count.Should().Be(3);
        Regex.Matches(exportResult, $"\r\n\r\n\r\n{personHeader}").Count.Should().Be(2);
        Regex.Matches(exportResult, $"\r\n\r\n{petHeader}").Count.Should().Be(2);
        Regex.Matches(exportResult, $"\r\n\r\n{customFieldHeader}").Count.Should().Be(1);
    }

    private async Task<string> GetExportAsync(string nickName)
    {
        var response = await $"{Url}/export/csv".WithAuth()
            .SetQueryParam("pagenumber", 1.ToString())
            .SetQueryParam("pageSize", 10.ToString())
            .SetQueryParam("doCount", true.ToString())
            .SetQueryParam("fileName", "temp")
            .SetQueryParam("nickName", nickName)
            .GetAsync();

        //assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(200);

        var responseString = await response.GetStringAsync();
        responseString.Should().NotBeNullOrWhiteSpace();
        return responseString;
    }

    private async Task<GetPersonViewModel> CreatePersonAsync(string nickName)
    {
        var faked = await GenerateCreateRequestAsync();
        faked.NickName = nickName;
        faked.Email = null;
        faked.OtherEmail = null;
        var response = await Url.WithAuth()
            .PostJsonAsync(faked);

        var responseModel = await response.GetJsonAsync<GetPersonViewModel>();
        return responseModel;
    }

    private async Task<GetPetViewModel> CreatePetAsync(Guid personId)
    {
        var faked = PetFaker.Faker.Generate();
        faked.DataAuth.UserEmails = ["developer@test.com"];
        var response = await $"{Url}/{personId}/pets".WithAuth()
            .PostJsonAsync(faked);

        var responseModel = await response.GetJsonAsync<GetPetViewModel>();
        return responseModel;
    }

    private async Task<CustomFieldViewModel> CreatePetCustomFieldAsync(Guid personId, Guid petId)
    {
        var faked = CustomFieldFaker.Faker.Generate();
        var response = await $"{Url}/{personId}/pets/{petId}/custom-fields".WithAuth().PostJsonAsync(faked);

        var responseModel = await response.GetJsonAsync<CustomFieldViewModel>();
        return responseModel;
    }

    protected override Task<PersonViewModelBase> GenerateCreateRequestAsync()
    {
        var faked = PersonFaker.Faker.Generate();
        faked.DataAuth.UserEmails = ["developer@test.com"];
        return Task.FromResult(faked);
    }

    protected override Task<PersonViewModelBase> GenerateUpdateRequestAsync(PersonViewModelBase createRequest)
    {
        var clone = createRequest.Clone();
        var faked = PersonFaker.Faker.Generate();
        clone.NickName = faked.NickName;
        return Task.FromResult(clone);
    }

    protected override Task<JsonPatchDocument> GeneratePatchAsync()
        => Task.FromResult(
            PatchFaker.MakeReplacePatch<PersonViewModelBase, string>(x => x.Email, new Faker().Person.Email));
}
