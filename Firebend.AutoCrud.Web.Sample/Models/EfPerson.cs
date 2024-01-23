using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.Models;

[Table("EfPeople")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(OtherEmail), IsUnique = true)]
public class EfPerson : IPerson, ITenantEntity<int>,
    ICustomFieldsEntity<Guid>, IEntityDataAuth
{
    public EfPerson()
    {
    }

    public EfPerson(CreatePersonViewModel viewModel)
    {
        viewModel.Body.CopyPropertiesTo(this);
    }

    public EfPerson(PersonViewModelBase viewModel)
    {
        viewModel.CopyPropertiesTo(this);
    }

    [StringLength(250)]
    public string FirstName { get; set; }

    [StringLength(250)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string NickName { get; set; }

    [StringLength(300)]
    public string Email { get; set; }

    [StringLength(300)]
    [Column("NotEmail")]
    public string OtherEmail { get; set; }

    public bool IsDeleted { get; set; }

    [Key] public Guid Id { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public int TenantId { get; set; }

    public ICollection<EfPet> Pets { get; set; }

    public List<CustomFieldsEntity<Guid>> CustomFields { get; set; }

    public DataAuth DataAuth { get; set; }
}
