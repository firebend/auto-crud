using Bogus;
using Firebend.AutoCrud.IntegrationTests.Models;

namespace Firebend.AutoCrud.IntegrationTests.Fakers
{
    public static class CustomFieldFaker
    {
        private static Faker<CustomFieldViewModel> _fakerViewModelBase;

        public static Faker<CustomFieldViewModel> Faker
        {
            get
            {
                _fakerViewModelBase ??= new Faker<CustomFieldViewModel>()
                    .StrictMode(true)
                    .RuleFor(x => x.Key, f => f.Commerce.ProductMaterial())
                    .RuleFor(x => x.Value, f => f.Commerce.Color());

                return _fakerViewModelBase;
            }
        }
    }
}
