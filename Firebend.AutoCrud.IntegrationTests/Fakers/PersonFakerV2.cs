using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers
{
    public static class PersonFakerV2
    {
        private static Faker<PersonViewModelBaseV2> _fakerViewModelBase;

        public static Faker<PersonViewModelBaseV2> Faker
        {
            get
            {
                _fakerViewModelBase ??= new Faker<PersonViewModelBaseV2>()
                    .StrictMode(true)
                    .RuleFor(x => x.Email, f => f.Person.Email)
                    .RuleFor(x => x.Name, _ => NameFaker.Faker.Generate())
                    .RuleFor(x => x.OtherEmail, f => f.Person.Email)
                    .RuleFor(x => x.DataAuth, _ => new DataAuth());

                return _fakerViewModelBase;
            }
        }
    }

    public static class NameFaker
    {
        private static Faker<Name> _fakerViewModelBase;

        public static Faker<Name> Faker
        {
            get
            {
                _fakerViewModelBase ??= new Faker<Name>()
                    .StrictMode(true)
                    .RuleFor(x => x.First, f => f.Person.FirstName)
                    .RuleFor(x => x.Last, f => f.Person.LastName)
                    .RuleFor(x => x.NickName, f => f.Person.UserName);

                return _fakerViewModelBase;
            }
        }
    }
}
