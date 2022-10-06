using System;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using FluentAssertions;
using Flurl.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class MongoEndToEndTest : BaseTest<
    Guid,
    PersonViewModelBase,
    PersonViewModelBase,
    GetPersonViewModel,
    PersonExport>
{
    protected override string Url => $"{TestConstants.BaseUrl}/v1/mongo-person";

    [TestMethod]
    public async Task Mongo_Api_Should_Work() => await EndToEndAsync(x => x.FirstName);


    [TestMethod]
    public async Task Validate_Endpoints_Should_Work_On_ViewModel()
    {
        await TestRunner.Authenticate();
        var person = await GenerateCreateRequestAsync();
        person.FirstName = "";
        try
        {
            var response = await $"{Url}/validate"
                .WithAuth()
                .PostJsonAsync(person);
            Assert.Fail($"Validate Post request should have return a 400 but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(400);
        }
        try
        {
            var response = await $"{Url}/{Guid.NewGuid()}/validate"
                .WithAuth()
                .PutJsonAsync(person);
            Assert.Fail($"Validate Put request should have return a 400 but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(400);
        }
    }

    [TestMethod]
    public async Task Validate_Endpoints_Should_Work_On_Model()
    {
        await TestRunner.Authenticate();
        var person = await GenerateCreateRequestAsync();
        person.LastName = "";
        try
        {
            var response = await $"{Url}/validate"
                .WithAuth()
                .PostJsonAsync(person);
            Assert.Fail($"Validate Post request should have return a 400 but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(400);
        }
        try
        {
            var response = await $"{Url}/{Guid.NewGuid()}/validate"
                .WithAuth()
                .PutJsonAsync(person);
            Assert.Fail($"Validate Put request should have return a 400 but instead was {response.StatusCode}");
        }
        catch (FlurlHttpException e)
        {
            e.StatusCode.Should().Be(400);
        }
    }

    [TestMethod]
    public async Task Validate_Endpoints_Should_Work()
    {
        await TestRunner.Authenticate();
        var person = await GenerateCreateRequestAsync();

        var postResponse = await $"{Url}/validate"
            .WithAuth()
            .PostJsonAsync(person);
        var putResponse = await $"{Url}/{Guid.NewGuid()}/validate"
            .WithAuth()
            .PutJsonAsync(person);

        postResponse.StatusCode.Should().Be(201);
        putResponse.StatusCode.Should().Be(200);
    }


    protected override Task<PersonViewModelBase> GenerateCreateRequestAsync()
    {
        var faked = PersonFaker.Faker.Generate();
        faked.DataAuth.UserEmails = new[] { "developer@test.com" };
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
        => Task.FromResult(PatchFaker.MakeReplacePatch<PersonViewModelBase, string>(x => x.FirstName, new Faker().Person.FirstName));
}
