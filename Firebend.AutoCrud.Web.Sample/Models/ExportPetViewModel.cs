using System;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Io.Attributes;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class ExportPetViewModel : IEntityViewModelExport
{
    [Export(Name = "Person Id", Order = 0)]
    public Guid? PersonId { get; set; }

    [Export(Name = "Person First Name", Order = 1)]
    public string PersonFirstName { get; set; }

    [Export(Name = "Person Last  Name", Order = 2)]
    public string PersonLastName { get; set; }

    [Export(Name = "Pet Id", Order = 3)]
    public Guid PetId { get; set; }

    [Export(Name = "Pet Name", Order = 4)]
    public string PetName { get; set; }

    [Export(Name = "Pet Type", Order = 5)]
    public string PetType { get; set; }

    [Export(Name = "Pet Created Date", Order = 6)]
    public DateTimeOffset CreatedDate { get; set; }

    [Export(Name = "Pet Modified Date", Order = 7)]
    public DateTimeOffset ModifiedDate { get; set; }

    [Export(Name = "Pet Deleted", Order = 8)]
    public bool IsDeleted { get; set; }

    public ExportPetViewModel()
    {

    }

    public ExportPetViewModel(EfPet pet)
    {
        pet.CopyPropertiesTo(this);
        PersonFirstName = pet.Person?.FirstName;
        PersonLastName = pet.Person?.LastName;
        PersonId = pet.Person?.Id;
        PetId = pet.Id;
    }

    public Guid Id { get; set; }
}
