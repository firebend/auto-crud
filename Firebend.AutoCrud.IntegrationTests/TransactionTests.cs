using System.Threading.Tasks;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using FluentAssertions;
using Flurl.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class TransactionTests
{
    private SessionTransactionRequestModel _requestModel;
    private GetPersonViewModel _efPerson;
    private GetPersonViewModel _mongoPerson;
    private const string TransactionsUrl = $"{TestConstants.BaseUrl}/v1/transactions";
    private const string EfPersonUrl = $"{TestConstants.BaseUrl}/v1/ef-person";
    private const string MongoPersonUrl = $"{TestConstants.BaseUrl}/v1/mongo-person";

    [TestInitialize]
    public async Task SeedData()
    {
        _efPerson = await CreatePersonAsync(EfPersonUrl);
        _efPerson.CustomFields = [];
        _mongoPerson = await CreatePersonAsync(MongoPersonUrl);
        _requestModel = new SessionTransactionRequestModel { EfPersonId = _efPerson.Id, MongoPersonId = _mongoPerson.Id };
    }

    private async Task<GetPersonViewModel> CreatePersonAsync(string personUrl)
    {
        var faked = PersonFaker.Faker.Generate();
        var response = await personUrl.WithAuth()
            .PostJsonAsync(faked);

        var responseModel = await response.GetJsonAsync<GetPersonViewModel>();
        responseModel.Should().NotBeNull();
        responseModel.Id.Should().NotBeEmpty();
        return responseModel;
    }

    [TestMethod]
    public async Task TestCommit()
    {
        var result = await $"{TransactionsUrl}/commit".WithAuth()
            .PostJsonAsync(_requestModel);

        var response = await result.GetJsonAsync<SessionTransactionAssertionViewModel>();

        response.Should().NotBeNull();
        response.Ef.Created.Should().NotBeNull();
        response.Ef.Created.Id.Should().NotBeEmpty();
        response.Ef.Read.Should().NotBeNull();
        response.Ef.Read.Id.Should().Be(_efPerson.Id);
        response.Ef.Read.Should().NotBeEquivalentTo(_efPerson);
        response.Ef.CreateWasCommitted.Should().BeTrue();
        response.Ef.PutWasCommitted.Should().BeTrue();
        response.Ef.PatchWasCommitted.Should().BeTrue();
        response.Ef.DeleteWasCommitted.Should().BeTrue();
        response.Ef.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.Mongo.Created.Should().NotBeNull();
        response.Mongo.Created.Id.Should().NotBeEmpty();
        response.Mongo.Read.Should().NotBeNull();
        response.Mongo.Read.Id.Should().Be(_mongoPerson.Id);
        response.Mongo.Read.Should().NotBeEquivalentTo(_mongoPerson);
        response.Mongo.CreateWasCommitted.Should().BeTrue();
        response.Mongo.PutWasCommitted.Should().BeTrue();
        response.Mongo.PatchWasCommitted.Should().BeTrue();
        response.Mongo.DeleteWasCommitted.Should().BeTrue();
        response.Mongo.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.ExceptionMessage.Should().BeEmpty();
    }

    [TestMethod]
    public async Task TestRollback()
    {
        var result = await $"{TransactionsUrl}/rollback".WithAuth()
            .PostJsonAsync(_requestModel);

        var response = await result.GetJsonAsync<SessionTransactionAssertionViewModel>();

        response.Should().NotBeNull();
        response.Ef.Created.Should().BeNull();
        response.Ef.Read.Should().NotBeNull();
        response.Ef.Read.Id.Should().Be(_efPerson.Id);
        response.Ef.Read.Should().BeEquivalentTo(_efPerson);
        response.Ef.CreateWasCommitted.Should().BeFalse();
        response.Ef.PutWasCommitted.Should().BeFalse();
        response.Ef.PatchWasCommitted.Should().BeFalse();
        response.Ef.DeleteWasCommitted.Should().BeFalse();
        response.Ef.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.Mongo.Created.Should().BeNull();
        response.Mongo.Read.Should().NotBeNull();
        response.Mongo.Read.Id.Should().Be(_mongoPerson.Id);
        response.Mongo.Read.Should().BeEquivalentTo(_mongoPerson);
        response.Mongo.CreateWasCommitted.Should().BeFalse();
        response.Mongo.PutWasCommitted.Should().BeFalse();
        response.Mongo.PatchWasCommitted.Should().BeFalse();
        response.Mongo.DeleteWasCommitted.Should().BeFalse();
        response.Mongo.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.ExceptionMessage.Should().BeEmpty();
    }

    [TestMethod]
    public async Task TestExceptionRollback()
    {
        var result = await $"{TransactionsUrl}/exception".WithAuth()
            .PostJsonAsync(_requestModel);

        var response = await result.GetJsonAsync<SessionTransactionAssertionViewModel>();

        response.Should().NotBeNull();
        response.Ef.Created.Should().BeNull();
        response.Ef.Read.Should().NotBeNull();
        response.Ef.Read.Id.Should().Be(_efPerson.Id);
        response.Ef.Read.Should().BeEquivalentTo(_efPerson);
        response.Ef.CreateWasCommitted.Should().BeFalse();
        response.Ef.PutWasCommitted.Should().BeFalse();
        response.Ef.PatchWasCommitted.Should().BeFalse();
        response.Ef.DeleteWasCommitted.Should().BeFalse();
        response.Ef.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.Mongo.Created.Should().BeNull();
        response.Mongo.Read.Should().NotBeNull();
        response.Mongo.Read.Id.Should().Be(_mongoPerson.Id);
        response.Mongo.Read.Should().BeEquivalentTo(_mongoPerson);
        response.Mongo.CreateWasCommitted.Should().BeFalse();
        response.Mongo.PutWasCommitted.Should().BeFalse();
        response.Mongo.PatchWasCommitted.Should().BeFalse();
        response.Mongo.DeleteWasCommitted.Should().BeFalse();
        response.Mongo.ChangesCanBeReadInTransaction.Should().BeTrue();

        response.ExceptionMessage.Should().NotBeEmpty();
    }
}
