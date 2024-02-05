using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PersonViewModelBase : IEntityViewModelBase
{
    public PersonViewModelBase()
    {
    }

    public PersonViewModelBase(EfPerson person)
    {
        person.CopyPropertiesTo(this);
    }

    public PersonViewModelBase(MongoPerson person)
    {
        person.CopyPropertiesTo(this);
    }

    [StringLength(250)]
    public string FirstName { get; set; }

    [StringLength(250)]
    [Required]
    public string LastName { get; set; }

    [StringLength(100)]
    public string NickName { get; set; }

    [StringLength(300)]
    public string Email { get; set; }

    [StringLength(300)]
    public string OtherEmail { get; set; }

    public DataAuth DataAuth { get; set; }
}
