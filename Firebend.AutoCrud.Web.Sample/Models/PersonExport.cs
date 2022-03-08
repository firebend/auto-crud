using System;
using Firebend.AutoCrud.Core.Extensions;

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
        }

        public PersonExport(MongoPerson person)
        {
            person.CopyPropertiesTo(this);
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public Guid Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
