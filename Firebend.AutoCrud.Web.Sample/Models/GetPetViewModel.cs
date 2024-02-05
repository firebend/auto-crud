using System;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class GetPetViewModel : IEntityViewModelRead<Guid>
{
    public bool IsDeleted { get; set; }
    public PetPersonViewModel Person { get; set; }

    public GetPetViewModel()
    {

    }

    public GetPetViewModel(EfPet pet)
    {
        pet.CopyPropertiesTo(this);

        if (pet.Person == null)
        {
            return;
        }

        this.Person = new PetPersonViewModel();
        pet.Person.CopyPropertiesTo(this.Person);
    }

    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
}
