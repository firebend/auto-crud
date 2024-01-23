using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PetExport : IEntityViewModelExport
{
    public PetExport()
    {

    }

    public PetExport(EfPet pet)
    {
        pet.CopyPropertiesTo(this);
        EfPersonId = pet.EfPersonId;
    }

    public Guid Id { get; set; }

    public Guid EfPersonId { get; set; }

    public string PetName { get; set; }

    public string PetType { get; set; }

    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
}
