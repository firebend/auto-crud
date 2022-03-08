using System;
using System.Threading.Tasks;
using Bogus;
using Firebend.AutoCrud.IntegrationTests.Fakers;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
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
        protected override string Url => $"{BaseUrl}/api/v1/mongo-person";

        [TestMethod]
        public async Task Mongo_Api_Should_Work() => await EndToEndAsync(x => x.FirstName);

        protected override Task<UserInfoPostDto> GenerateAuthenticateRequestAsync()
            => Task.FromResult(new UserInfoPostDto{ Email = "developer@test.com", Password = "password" });

        protected override Task<PersonViewModelBase> GenerateCreateRequestAsync()
        {
            var faked = PersonFaker.Faker.Generate();
            faked.DataAuth.UserEmails = new[] {"developer@test.com"};
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
}
