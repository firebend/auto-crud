using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Extensions;
using MassTransit.Futures.Contracts;

namespace Firebend.AutoCrud.Web.Sample.Models
{
    public class PersonExport : IEntityViewModelExport
    {
        public PersonExport()
        {
        }

        public PersonExport(EfPerson person)
        {
            person.CopyPropertiesTo(this);
            if (FirstName == "George" || FirstName == "Bob")
            {
                Pets = new List<PetExport>
                {
                    new() { Id = Guid.NewGuid(), PetName = "ted" },
                    new() { Id = Guid.NewGuid(), PetName = "derp" }
                };

            }
        }

        public PersonExport(MongoPerson person)
        {
            person.CopyPropertiesTo(this);
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<PetExport> Pets { get; set; }
        public ICollection<EfPet> Pets1 { get; set; }

        public Guid Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
