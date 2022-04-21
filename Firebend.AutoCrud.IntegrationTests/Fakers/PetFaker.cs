using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers
{
    public static class PetFaker
    {
        private static Faker<PetBaseViewModel> _fakerViewModelBase;

        public static Faker<PetBaseViewModel> Faker
        {
            get
            {
                _fakerViewModelBase ??= new Faker<PetBaseViewModel>()
                    .StrictMode(true)
                    .RuleFor(x => x.PetName, f => f.Person.FirstName)
                    .RuleFor(x => x.PetType, f => f.Hacker.Verb())
                    .RuleFor(x => x.DataAuth, f => new DataAuth { UserEmails = new[] { f.Person.Email } });

                return _fakerViewModelBase;
            }
        }
    }
}
