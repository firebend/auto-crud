using System;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests
{
    [TestClass]
    public class EfEndToEndTest : BaseTest<
        Guid,
        CreatePersonViewModel,
        PersonViewModelBase,
        GetPersonViewModel,
        PersonExport>
    {
        public override string Url => "http://localhost:5000/api/v1/ef-person";

        [TestMethod]
        public async Task Ef_Api_Should_Work() => await EndToEndAsync(x => x.FirstName);

        public override Task<CreatePersonViewModel> GenerateCreateRequestAsync()
            => Task.FromResult(new CreatePersonViewModel {Body = PersonFaker.Faker.Generate()});

        protected override Task<PersonViewModelBase> GenerateUpdateRequestAsync(CreatePersonViewModel createRequest)
            => Task.FromResult(PersonFaker.Faker.Generate());

        protected override Task<JsonPatchDocument> GeneratePatchAsync()
            => Task.FromResult(
                PatchFaker.MakeReplacePatch<PersonViewModelBase, string>(x => x.Email, new Faker().Person.Email));

        protected override Task<UserInfoPostDto> GenerateAuthenticateRequestAsync() =>
            throw new NotImplementedException();
    }
}
