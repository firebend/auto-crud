using Bogus;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class UserFaker
{
    private static Faker<UserInfoPostDto> _faker;

    public static Faker<UserInfoPostDto> Faker
    {
        get
        {
            _faker ??= new Faker<UserInfoPostDto>()
                .StrictMode(false)
                .RuleFor(x => x.Email, f => f.Person.Email)
                .RuleFor(x => x.Password, f => f.Internet.Password());

            return _faker;
        }
    }
}
