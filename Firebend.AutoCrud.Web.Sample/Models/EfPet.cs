using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class EfPet : IEntity<Guid>, IModifiedEntity, ITenantEntity<int>, IActiveEntity, ICustomFieldsEntity<Guid>,
    IEntityDataAuth
{
    public EfPet()
    {
    }

    public EfPet(CreatePetViewModel pet)
    {
        pet.Body.CopyPropertiesTo(this);
        EfPersonId = pet.PersonId;
    }

    public EfPet(PutPetViewModel pet)
    {
        pet.CopyPropertiesTo(this);
    }

    public Guid Id { get; set; }

    public Guid EfPersonId { get; set; }

    public EfPerson Person { get; set; }

    [Required]
    [MaxLength(500)]
    public string PetName { get; set; }

    [Required]
    [MaxLength(250)]
    public string PetType { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public int TenantId { get; set; }
    public bool IsDeleted { get; set; }
    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }
    public DataAuth DataAuth { get; set; }
}
