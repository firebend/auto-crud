namespace Firebend.AutoCrud.Web.Sample.Models;

public static class PersonExtensions
{
    public static GetPersonViewModel ToViewModel(this EfPerson person) => new(person);
    public static GetPersonViewModel ToViewModel(this MongoTenantPerson person) => new(person);
}
