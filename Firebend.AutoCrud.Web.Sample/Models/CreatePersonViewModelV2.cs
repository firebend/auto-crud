using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CreatePersonViewModelV2 : IEntityViewModelCreate<PersonViewModelBaseV2>, IViewModelWithBody<PersonViewModelBaseV2>
{
    public CreatePersonViewModelV2()
    {

    }

    public CreatePersonViewModelV2(EfPerson entity)
    {
        Body = new PersonViewModelBaseV2(entity);
    }

    public CreatePersonViewModelV2(MongoPerson entity)
    {
        Body = new PersonViewModelBaseV2(entity);
    }

    [FromBody]
    public PersonViewModelBaseV2 Body { get; set; }
}
