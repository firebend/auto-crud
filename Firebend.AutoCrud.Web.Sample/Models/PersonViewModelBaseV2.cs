using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PersonViewModelBaseV2 : IEntityViewModelBase
{
    public PersonViewModelBaseV2()
    {
    }

    public PersonViewModelBaseV2(EfPerson person)
    {
        person.CopyPropertiesTo(this);
        Name = new Name
        {
            First = person.FirstName,
            Last = person.LastName,
            NickName = person.NickName
        };
    }

    public PersonViewModelBaseV2(MongoPerson person)
    {
        person.CopyPropertiesTo(this);
        Name = new Name
        {
            First = person.FirstName,
            Last = person.LastName,
            NickName = person.NickName
        };
    }

    public Name Name { get; set; }

    [StringLength(300)]
    public string Email { get; set; }

    [StringLength(300)]
    public string OtherEmail { get; set; }

    public DataAuth DataAuth { get; set; }
}
