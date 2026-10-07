using Bogus;
using Firebend.AutoCrud.CustomFields.Web.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers;

public static class CustomFieldFaker
{
    public static Faker<CustomFieldViewModelCreate> Faker
    {
        get
        {
            field ??= new Faker<CustomFieldViewModelCreate>()
                .StrictMode(true)
                .RuleFor(x => x.Key, f => f.Commerce.ProductMaterial())
                .RuleFor(x => x.Value, f => f.Commerce.Color());

            return field;
        }
    }
}
