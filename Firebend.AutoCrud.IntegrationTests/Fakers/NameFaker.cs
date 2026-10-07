using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class NameFaker
{
    public static Faker<Name> Faker
    {
        get
        {
            field ??= new Faker<Name>()
                .StrictMode(true)
                .RuleFor(x => x.First, f => f.Person.FirstName)
                .RuleFor(x => x.Last, f => f.Person.LastName)
                .RuleFor(x => x.NickName, f => f.Person.UserName);

            return field;
        }
    }
}
