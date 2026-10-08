using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class PersonFakerV2
{
    public static Faker<PersonViewModelBaseV2> Faker
    {
        get
        {
            field ??= new Faker<PersonViewModelBaseV2>()
                .StrictMode(true)
                .RuleFor(x => x.Email, f => f.Person.Email)
                .RuleFor(x => x.Name, _ => NameFaker.Faker.Generate())
                .RuleFor(x => x.OtherEmail, f => f.Person.Email)
                .RuleFor(x => x.DataAuth, _ => new DataAuth());

            return field;
        }
    }
}
