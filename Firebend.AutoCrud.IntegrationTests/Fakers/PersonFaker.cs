using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class PersonFaker
{
    public static Faker<PersonViewModelBase> Faker
    {
        get
        {
            field ??= new Faker<PersonViewModelBase>()
                .StrictMode(true)
                .RuleFor(x => x.Email, f => f.Person.Email)
                .RuleFor(x => x.FirstName, f => f.Person.FirstName)
                .RuleFor(x => x.LastName, f => f.Person.LastName)
                .RuleFor(x => x.NickName, f => f.Person.UserName)
                .RuleFor(x => x.OtherEmail, f => f.Person.Email)
                .RuleFor(x => x.DataAuth, _ => new DataAuth());

            return field;
        }
    }
}
