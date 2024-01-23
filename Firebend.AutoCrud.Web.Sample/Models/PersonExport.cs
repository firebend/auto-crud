using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class PersonExport : IEntityViewModelExport
{
    public PersonExport()
    {
    }

    public PersonExport(EfPerson person)
    {
        person.CopyPropertiesTo(this);
        Pets = person.Pets.Select(x => new PetExport(x)).ToList();
    }

    public PersonExport(MongoPerson person)
    {
        person.CopyPropertiesTo(this);
    }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public List<PetExport> Pets { get; set; }

    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
}
