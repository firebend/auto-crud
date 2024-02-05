using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

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
