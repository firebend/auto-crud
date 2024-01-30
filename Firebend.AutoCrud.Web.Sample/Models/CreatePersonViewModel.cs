using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CreatePersonViewModel : IEntityViewModelCreate<PersonViewModelBase>, IViewModelWithBody<PersonViewModelBase>
{
    public CreatePersonViewModel()
    {

    }

    public CreatePersonViewModel(EfPerson entity)
    {
        Body = new PersonViewModelBase(entity);
    }

    public CreatePersonViewModel(MongoPerson entity)
    {
        Body = new PersonViewModelBase(entity);
    }

    [FromBody]
    public PersonViewModelBase Body { get; set; }
}
