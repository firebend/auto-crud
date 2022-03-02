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
    public class MongoEndToEndTest : BaseTest<
        Guid,
        PersonViewModelBase,
        PersonViewModelBase,
        GetPersonViewModel,
        PersonExport>
    {
        public override string Url => "http://localhost:5000/api/v1/mongo-person";

        [TestMethod]
        public async Task Mongo_Api_Should_Work() => await EndToEndAsync(x => x.FirstName);

        public override Task<PersonViewModelBase> GenerateCreateRequestAsync()
            => Task.FromResult(PersonFaker.Faker.Generate());

        protected override Task<PersonViewModelBase> GenerateUpdateRequestAsync(PersonViewModelBase createRequest)
            => Task.FromResult(PersonFaker.Faker.Generate());

        protected override Task<JsonPatchDocument> GeneratePatchAsync()
            => Task.FromResult(PatchFaker.MakeReplacePatch<PersonViewModelBase, string>(x => x.FirstName, new Faker().Person.FirstName));
    }
}
